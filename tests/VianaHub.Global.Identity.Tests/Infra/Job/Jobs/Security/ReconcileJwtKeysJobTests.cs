using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Job.Jobs.Security;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Jobs.Security;

public class ReconcileJwtKeysJobTests
{
    private readonly Mock<IJwtKeyDomainService> _jwtKeyDomainMock = new();
    private readonly Mock<ITenantDataRepository> _tenantRepoMock = new();
    private readonly Mock<IJwtKeyDataRepository> _jwtKeyRepoMock = new();
    private readonly Mock<IRequestTenantContext> _tenantContextMock = new();
    private readonly Mock<ILogger<ReconcileJwtKeysJob>> _loggerMock = new();

    private ReconcileJwtKeysJob CreateSut() =>
        new(_jwtKeyDomainMock.Object,
            _tenantRepoMock.Object,
            _jwtKeyRepoMock.Object,
            _tenantContextMock.Object,
            _loggerMock.Object);

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

    [Fact(DisplayName = "Execute - Deve reconciliar chaves para todos os tenants ativos")]
    [Trait("Infra.Job", "")]
    public async Task Execute_Sucesso_DeveProcessarTodosTenantsAtivos()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1), BuildTenant(2) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(BuildJwtKey());

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyRepoMock.Verify(x => x.GetActiveKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Execute - Deve criar chave JWT quando tenant não possui chave ativa")]
    [Trait("Infra.Job", "")]
    public async Task Execute_TenantSemChave_DeveCriarChaveViaDomainService()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((JwtKeyEntity)null!);
        _jwtKeyDomainMock.Setup(x => x.EnsureKeyExistsAsync(1, 0, It.IsAny<CancellationToken>())).ReturnsAsync(BuildJwtKey());

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyDomainMock.Verify(x => x.EnsureKeyExistsAsync(1, 0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Execute - Deve pular tenant quando chave JWT ativa já existe")]
    [Trait("Infra.Job", "")]
    public async Task Execute_TenantComChaveAtiva_DevePular()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildJwtKey());

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyDomainMock.Verify(x => x.EnsureKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Execute - Deve ignorar tenants inativos")]
    [Trait("Infra.Job", "")]
    public async Task Execute_TenantInativo_DeveIgnorar()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1, isActive: false) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyRepoMock.Verify(x => x.GetActiveKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Execute - Deve ignorar tenants excluídos")]
    [Trait("Infra.Job", "")]
    public async Task Execute_TenantExcluido_DeveIgnorar()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1, isDeleted: true) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jwtKeyRepoMock.Verify(x => x.GetActiveKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Execute - Deve propagar exceção quando ProcessTenantAsync falha")]
    [Trait("Infra.Job", "")]
    public async Task Execute_ExcecaoEmUmTenant_DevePropagarExcecao()
    {
        var tenants = new List<TenantEntity> { BuildTenant(1), BuildTenant(2) };
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tenants);
        _jwtKeyRepoMock.Setup(x => x.GetActiveKeyAsync(1, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Erro tenant 1"));

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.Execute(CancellationToken.None));

        Assert.NotNull(exception);
        Assert.Equal("Erro tenant 1", exception.Message);
    }

    [Fact(DisplayName = "Execute - Deve completar sem erros quando não há tenants")]
    [Trait("Infra.Job", "")]
    public async Task Execute_SemTenants_DeveCompletarSemErros()
    {
        _tenantRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.Execute(CancellationToken.None));

        Assert.Null(exception);
        _jwtKeyRepoMock.Verify(x => x.GetActiveKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
