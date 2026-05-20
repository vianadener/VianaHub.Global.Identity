using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Services;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain;

public class JwtKeyDomainServiceTests
{
    private readonly Mock<IJwtKeyDataRepository> _repoMock = new();
    private readonly Mock<ITenantDataRepository> _tenantRepoMock = new();
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IEntityDomainValidator<JwtKeyEntity>> _validatorMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ISecretProvider> _secretProviderMock = new();

    private const int TenantId = 1;
    private const int UserId = 10;

    public JwtKeyDomainServiceTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);
    }

    private JwtKeyDomainService CreateSut() => new(
        _repoMock.Object,
        _tenantRepoMock.Object,
        _notifyMock.Object,
        _validatorMock.Object,
        _localizationMock.Object,
        _currentUserMock.Object,
        NullLogger<JwtKeyDomainService>.Instance,
        _secretProviderMock.Object);

    private static JwtKeyEntity BuildJwtKey(int id = 1, bool isActive = true)
    {
        var entity = new JwtKeyEntity(TenantId, "publicKey", "encryptedPrivateKey", UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive)
        {
            entity.Deactivate(UserId);
        }
        return entity;
    }

    private static ValidationResult ValidResult() => new();

    private static ValidationResult InvalidResult(string error = "Erro de validação") =>
        new(new List<ValidationFailure> { new("Field", error) });

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar JwtKey quando encontrada")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarJwtKey()
    {
        var entity = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal(entity, result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando não encontrada")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByKeyIdAsync

    [Fact(DisplayName = "GetByKeyIdAsync - Deve retornar JwtKey quando encontrada pelo KeyId")]
    [Trait("Domain", "")]
    public async Task GetByKeyIdAsync_Sucesso_DeveRetornarJwtKey()
    {
        var keyId = Guid.NewGuid();
        var entity = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByKeyIdAsync(keyId, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.GetByKeyIdAsync(keyId, default);

        Assert.NotNull(result);
        Assert.Equal(entity, result);
    }

    [Fact(DisplayName = "GetByKeyIdAsync - Deve retornar null quando não encontrada")]
    [Trait("Domain", "")]
    public async Task GetByKeyIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var keyId = Guid.NewGuid();
        _repoMock.Setup(x => x.GetByKeyIdAsync(keyId, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.GetByKeyIdAsync(keyId, default);

        Assert.Null(result);
    }

    #endregion

    #region GetActiveKeyAsync

    [Fact(DisplayName = "GetActiveKeyAsync - Deve retornar chave ativa")]
    [Trait("Domain", "")]
    public async Task GetActiveKeyAsync_Sucesso_DeveRetornarChaveAtiva()
    {
        var entity = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.GetActiveKeyAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Equal(entity, result);
    }

    [Fact(DisplayName = "GetActiveKeyAsync - Deve retornar null quando não há chave ativa")]
    [Trait("Domain", "")]
    public async Task GetActiveKeyAsync_SemChaveAtiva_DeveRetornarNull()
    {
        _repoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.GetActiveKeyAsync(TenantId, default);

        Assert.Null(result);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar todas as chaves")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<JwtKeyEntity> { BuildJwtKey(1), BuildJwtKey(2) };
        _repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(entities);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region GetByTenantAsync

    [Fact(DisplayName = "GetByTenantAsync - Deve retornar chaves do tenant")]
    [Trait("Domain", "")]
    public async Task GetByTenantAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<JwtKeyEntity> { BuildJwtKey(1) };
        _repoMock.Setup(x => x.GetByTenantAsync(TenantId, default)).ReturnsAsync(entities);

        var sut = CreateSut();
        var result = await sut.GetByTenantAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve retornar null quando tenant não encontrado")]
    [Trait("Domain", "")]
    public async Task CreateAsync_TenantNaoEncontrado_DeveRetornarNull()
    {
        var entity = BuildJwtKey();
        _tenantRepoMock.Setup(x => x.GetByIdAsync(TenantId, default)).ReturnsAsync((TenantEntity)null!);

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar null quando validação falha")]
    [Trait("Domain", "")]
    public async Task CreateAsync_ValidacaoFalha_DeveRetornarNull()
    {
        var entity = BuildJwtKey();
        var tenant = new TenantEntity("Tenant", "Desc", "alias", null, null, null, UserId);
        _tenantRepoMock.Setup(x => x.GetByIdAsync(TenantId, default)).ReturnsAsync(tenant);
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar null quando já existe chave ativa")]
    [Trait("Domain", "")]
    public async Task CreateAsync_ChaveAtivaJaExiste_DeveRetornarNull()
    {
        var entity = BuildJwtKey();
        var tenant = new TenantEntity("Tenant", "Desc", "alias", null, null, null, UserId);
        _tenantRepoMock.Setup(x => x.GetByIdAsync(TenantId, default)).ReturnsAsync(tenant);
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.HasActiveKeyAsync(TenantId, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 409), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar null quando repositório falha ao persistir")]
    [Trait("Domain", "")]
    public async Task CreateAsync_FalhaAoPersistir_DeveRetornarNull()
    {
        var entity = BuildJwtKey();
        var tenant = new TenantEntity("Tenant", "Desc", "alias", null, null, null, UserId);
        _tenantRepoMock.Setup(x => x.GetByIdAsync(TenantId, default)).ReturnsAsync(tenant);
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.HasActiveKeyAsync(TenantId, default)).ReturnsAsync(false);
        _repoMock.Setup(x => x.CreateAsync(entity, default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve criar JwtKey com sucesso")]
    [Trait("Domain", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarEntidade()
    {
        var entity = BuildJwtKey();
        var tenant = new TenantEntity("Tenant", "Desc", "alias", null, null, null, UserId);
        _tenantRepoMock.Setup(x => x.GetByIdAsync(TenantId, default)).ReturnsAsync(tenant);
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.HasActiveKeyAsync(TenantId, default)).ReturnsAsync(false);
        _repoMock.Setup(x => x.CreateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.NotNull(result);
        Assert.Equal(entity, result);
    }

    #endregion

    #region ActivateAsync

    [Fact(DisplayName = "ActivateAsync - Deve retornar false quando chave não encontrada")]
    [Trait("Domain", "")]
    public async Task ActivateAsync_NaoEncontrada_DeveRetornarFalse()
    {
        var key = BuildJwtKey(99);
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(key, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 404), Times.Once);
    }

    [Fact(DisplayName = "ActivateAsync - Deve retornar false quando validação falha")]
    [Trait("Domain", "")]
    public async Task ActivateAsync_ValidacaoFalha_DeveRetornarFalse()
    {
        var key = BuildJwtKey(1);
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _validatorMock.Setup(x => x.ValidateForActivateAsync(existing)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.ActivateAsync(key, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "ActivateAsync - Deve ativar chave com sucesso")]
    [Trait("Domain", "")]
    public async Task ActivateAsync_Sucesso_DeveRetornarTrue()
    {
        var key = BuildJwtKey(1);
        var existing = BuildJwtKey(1, isActive: false);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _validatorMock.Setup(x => x.ValidateForActivateAsync(existing)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.GetActiveKeyAsync(existing.TenantId, default)).ReturnsAsync((JwtKeyEntity)null!);
        _repoMock.Setup(x => x.UpdateAsync(existing, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(key, default);

        Assert.True(result);
    }

    #endregion

    #region DeactivateAsync

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false quando chave não encontrada")]
    [Trait("Domain", "")]
    public async Task DeactivateAsync_NaoEncontrada_DeveRetornarFalse()
    {
        var key = BuildJwtKey(99);
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(key, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 404), Times.Once);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false quando validação falha")]
    [Trait("Domain", "")]
    public async Task DeactivateAsync_ValidacaoFalha_DeveRetornarFalse()
    {
        var key = BuildJwtKey(1);
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _validatorMock.Setup(x => x.ValidateForDeactivateAsync(existing)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(key, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve desativar chave com sucesso")]
    [Trait("Domain", "")]
    public async Task DeactivateAsync_Sucesso_DeveRetornarTrue()
    {
        var key = BuildJwtKey(1);
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _validatorMock.Setup(x => x.ValidateForDeactivateAsync(existing)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.UpdateAsync(existing, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(key, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(existing, default), Times.Once);
    }

    #endregion

    #region RevokeAsync

    [Fact(DisplayName = "RevokeAsync - Deve retornar false quando chave não encontrada")]
    [Trait("Domain", "")]
    public async Task RevokeAsync_NaoEncontrada_DeveRetornarFalse()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.RevokeAsync(99, "motivo", UserId, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 404), Times.Once);
    }

    [Fact(DisplayName = "RevokeAsync - Deve retornar false quando validação falha")]
    [Trait("Domain", "")]
    public async Task RevokeAsync_ValidacaoFalha_DeveRetornarFalse()
    {
        var existing = BuildJwtKey(1, isActive: false);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _validatorMock.Setup(x => x.ValidateForRevokeAsync(existing)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.RevokeAsync(1, "motivo", UserId, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve retornar false quando chave não encontrada")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_NaoEncontrada_DeveRetornarFalse()
    {
        var key = BuildJwtKey(99);
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(key, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 404), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false quando validação falha")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_ValidacaoFalha_DeveRetornarFalse()
    {
        var key = BuildJwtKey(1);
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(existing)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeleteAsync(key, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve deletar chave com sucesso")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var key = BuildJwtKey(1);
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(existing)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.UpdateAsync(existing, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(key, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(existing, default), Times.Once);
    }

    #endregion

    #region UpdateRotationPolicyAsync

    [Fact(DisplayName = "UpdateRotationPolicyAsync - Deve retornar false quando chave não encontrada")]
    [Trait("Domain", "")]
    public async Task UpdateRotationPolicyAsync_NaoEncontrada_DeveRetornarFalse()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JwtKeyEntity)null!);

        var sut = CreateSut();
        var result = await sut.UpdateRotationPolicyAsync(99, 90, 7, UserId, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 404), Times.Once);
    }

    [Fact(DisplayName = "UpdateRotationPolicyAsync - Deve retornar false quando rotationPolicyDays inválido")]
    [Trait("Domain", "")]
    public async Task UpdateRotationPolicyAsync_RotacaoInvalida_DeveRetornarFalse()
    {
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);

        var sut = CreateSut();
        var result = await sut.UpdateRotationPolicyAsync(1, 10, 7, UserId, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "UpdateRotationPolicyAsync - Deve retornar false quando overlapPeriodDays inválido")]
    [Trait("Domain", "")]
    public async Task UpdateRotationPolicyAsync_OverlapInvalido_DeveRetornarFalse()
    {
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);

        var sut = CreateSut();
        var result = await sut.UpdateRotationPolicyAsync(1, 90, 0, UserId, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "UpdateRotationPolicyAsync - Deve retornar false quando overlap maior que rotação")]
    [Trait("Domain", "")]
    public async Task UpdateRotationPolicyAsync_OverlapMaiorQueRotacao_DeveRetornarFalse()
    {
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);

        var sut = CreateSut();
        var result = await sut.UpdateRotationPolicyAsync(1, 30, 30, UserId, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "UpdateRotationPolicyAsync - Deve atualizar política com sucesso")]
    [Trait("Domain", "")]
    public async Task UpdateRotationPolicyAsync_Sucesso_DeveRetornarTrue()
    {
        var existing = BuildJwtKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(existing);
        _repoMock.Setup(x => x.UpdateAsync(existing, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdateRotationPolicyAsync(1, 90, 7, UserId, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(existing, default), Times.Once);
    }

    #endregion
}
