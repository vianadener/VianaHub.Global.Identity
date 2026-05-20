using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Job.Jobs.Security;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Jobs.Security;

public class JwtKeyRotationJobTests
{
    private readonly Mock<IJwtKeyDomainService> _jwtKeyServiceMock = new();
    private readonly Mock<ITenantDataRepository> _tenantRepoMock = new();
    private readonly Mock<ILogger<JwtKeyRotationJob>> _loggerMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<IRequestTenantContext> _tenantContextMock = new();

    private JwtKeyRotationJob CreateSut()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("msg");
        return new(
            _jwtKeyServiceMock.Object,
            _tenantRepoMock.Object,
            _loggerMock.Object,
            _localizationMock.Object,
            _tenantContextMock.Object);
    }

    private static TenantEntity BuildTenant(int id = 1, bool isActive = true, bool isDeleted = false)
    {
        var tenant = new TenantEntity("Tenant", "Descrição", "alias", null, null, null, 1);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity).GetProperty("Id")!.SetValue(tenant, id);
        if (!isActive) tenant.Deactivate(1);
        if (isDeleted) tenant.Delete(1);
        return tenant;
    }

    private static JwtKeyEntity BuildJwtKey(int tenantId = 1) =>
        new(tenantId, "pub-key", "priv-key-encrypted", 1);

    #region Execute

    [Fact(DisplayName = "Execute - Deve rotacionar chaves para todos tenants ativos com sucesso")]
    [Trait("Infra.Job", "")]
    public async Task Execute_Sucesso_DeveRotacionarChavesParaTodosTenantsAtivos()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1), BuildTenant(2) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);
        _jwtKeyServiceMock.Setup(x => x.GetActiveKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(BuildJwtKey());
        _jwtKeyServiceMock.Setup(x => x.RotateKeysAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyServiceMock.Verify(x => x.RotateKeysAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Execute - Deve criar chave quando tenant não possui chave ativa")]
    [Trait("Infra.Job", "")]
    public async Task Execute_TenantSemChaveAtiva_DeveCriarChave()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);
        _jwtKeyServiceMock.Setup(x => x.GetActiveKeyAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((JwtKeyEntity)null!);
        _jwtKeyServiceMock.Setup(x => x.EnsureKeyExistsAsync(1, 0, It.IsAny<CancellationToken>())).ReturnsAsync(BuildJwtKey());
        _jwtKeyServiceMock.Setup(x => x.RotateKeysAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyServiceMock.Verify(x => x.EnsureKeyExistsAsync(1, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Execute - Deve ignorar tenants inativos")]
    [Trait("Infra.Job", "")]
    public async Task Execute_TenantInativo_DeveIgnorar()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1, isActive: false) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyServiceMock.Verify(x => x.RotateKeysAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Execute - Deve ignorar tenants excluídos")]
    [Trait("Infra.Job", "")]
    public async Task Execute_TenantExcluido_DeveIgnorar()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1, isDeleted: true) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyServiceMock.Verify(x => x.RotateKeysAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Execute - Deve propagar exceção quando repositório falha")]
    [Trait("Infra.Job", "")]
    public async Task Execute_ExcecaoNoRepositorio_DevePropagarExcecao()
    {
        _tenantRepoMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha no banco"));

        var sut = CreateSut();
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Execute(CancellationToken.None));
    }

    [Fact(DisplayName = "Execute - Deve continuar rotação para demais tenants quando um falha")]
    [Trait("Infra.Job", "")]
    public async Task Execute_ExcecaoEmUmTenant_DeveContinuarParaOsOutros()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1), BuildTenant(2) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);
        _jwtKeyServiceMock.Setup(x => x.GetActiveKeyAsync(1, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Erro tenant 1"));
        _jwtKeyServiceMock.Setup(x => x.GetActiveKeyAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(BuildJwtKey(2));
        _jwtKeyServiceMock.Setup(x => x.RotateKeysAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.Execute(CancellationToken.None));

        Assert.Null(exception);
        _jwtKeyServiceMock.Verify(x => x.RotateKeysAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    #endregion
}
