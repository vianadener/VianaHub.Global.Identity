using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.Jwt;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.Jwt;

public class JwtKeyValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private JwtKeyValidator CreateSut() => new(_localizationMock.Object);

    private static JwtKeyEntity BuildJwtKey(
        int tenantId = 1,
        string publicKey = "publickey_value",
        string privateKeyEncrypted = "encryptedprivatekey_value",
        string algorithm = "RS256",
        int keySize = 2048,
        bool isActive = true,
        bool isDeleted = false)
    {
        var entity = new JwtKeyEntity(tenantId, publicKey, privateKeyEncrypted, 10, algorithm, keySize);
        if (!isActive)
            entity.Deactivate(10);
        if (isDeleted)
            entity.Delete(10);
        return entity;
    }

    public JwtKeyValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    #region ValidateForCreateAsync

    [Fact(DisplayName = "ValidateForCreateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildJwtKey();
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando entity é null")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_EntityNull_DeveRetornarErro()
    {
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(null!);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildJwtKey(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando PublicKey é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_PublicKeyVazio_DeveRetornarErro()
    {
        var entity = BuildJwtKey(publicKey: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PublicKey");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando PrivateKeyEncrypted é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_PrivateKeyVazio_DeveRetornarErro()
    {
        var entity = BuildJwtKey(privateKeyEncrypted: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PrivateKeyEncrypted");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Algorithm é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_AlgorithmVazio_DeveRetornarErro()
    {
        var entity = BuildJwtKey(algorithm: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Algorithm");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando KeySize é menor que 1024")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_KeySizeMenorQue1024_DeveRetornarErro()
    {
        var entity = BuildJwtKey(keySize: 512);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "KeySize");
    }

    #endregion

    #region ValidateForUpdateAsync

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildJwtKey();
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando entity é null")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_EntityNull_DeveRetornarErro()
    {
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(null!);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildJwtKey(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    #endregion

    #region ValidateForActivateAsync

    [Fact(DisplayName = "ValidateForActivateAsync - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var entity = BuildJwtKey(isActive: false);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando entity é null")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntityNull_DeveRetornarErro()
    {
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(null!);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildJwtKey(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    #endregion

    #region ValidateForDeactivateAsync

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve sempre retornar sucesso")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_Sempre_DeveRetornarSucesso()
    {
        var entity = BuildJwtKey();
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.True(result.IsValid);
    }

    #endregion

    #region ValidateForDeleteAsync

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve ser válido com entidade inativa")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntidadeInativa_DeveRetornarSucesso()
    {
        var entity = BuildJwtKey(isActive: false);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando entity é null")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntityNull_DeveRetornarErro()
    {
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(null!);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando entidade está ativa")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntidadeAtiva_DeveRetornarErro()
    {
        var entity = BuildJwtKey(isActive: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsActive");
    }

    #endregion

    #region ValidateForRevokeAsync

    [Fact(DisplayName = "ValidateForRevokeAsync - Deve ser válido com entidade não revogada")]
    [Trait("Domain", "")]
    public async Task ValidateForRevokeAsync_EntidadeNaoRevogada_DeveRetornarSucesso()
    {
        var entity = BuildJwtKey();
        var sut = CreateSut();

        var result = await sut.ValidateForRevokeAsync(entity);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "ValidateForRevokeAsync - Deve falhar quando entity é null")]
    [Trait("Domain", "")]
    public async Task ValidateForRevokeAsync_EntityNull_DeveRetornarErro()
    {
        var sut = CreateSut();

        var result = await sut.ValidateForRevokeAsync(null!);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact(DisplayName = "ValidateForRevokeAsync - Deve falhar quando entidade já está revogada")]
    [Trait("Domain", "")]
    public async Task ValidateForRevokeAsync_EntidadeJaRevogada_DeveRetornarErro()
    {
        var entity = BuildJwtKey();
        entity.Revoke("Motivo de revogação", 10);
        var sut = CreateSut();

        var result = await sut.ValidateForRevokeAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsRevoked");
    }

    #endregion
}
