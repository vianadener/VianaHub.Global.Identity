using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.User;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.User;

public class UserValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private UserValidator CreateSut() => new(_localizationMock.Object);

    private static UserEntity BuildUser(
        int id = 1,
        int tenantId = 1,
        string name = "Usuário Teste",
        string passwordHash = "hashedpassword_at_least_sixty_characters_long_XXXXXXXXXXXXXXXXXX",
        bool isActive = true,
        bool isDeleted = false)
    {
        var entity = new UserEntity(tenantId, name, "login@test.com", passwordHash, null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive)
            entity.Deactivate(10);
        if (isDeleted)
            entity.Delete(10);
        return entity;
    }

    public UserValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    #region ValidateForCreateAsync

    [Fact(DisplayName = "ValidateForCreateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildUser();
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildUser(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_NameVazio_DeveRetornarErro()
    {
        var entity = BuildUser(name: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Name excede 150 caracteres")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_NameExcede150Chars_DeveRetornarErro()
    {
        var entity = BuildUser(name: new string('A', 151));
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando PasswordHash é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_PasswordHashVazio_DeveRetornarErro()
    {
        var entity = BuildUser(passwordHash: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando PasswordHash tem menos de 60 caracteres")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_PasswordHashMenorQue60Chars_DeveRetornarErro()
    {
        var entity = BuildUser(passwordHash: new string('x', 59));
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }

    #endregion

    #region ValidateForUpdateAsync

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildUser();
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildUser(id: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildUser(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_NameVazio_DeveRetornarErro()
    {
        var entity = BuildUser(name: "");
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    #endregion

    #region ValidateForActivateAsync

    [Fact(DisplayName = "ValidateForActivateAsync - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var entity = BuildUser(isActive: false);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildUser(isActive: false, isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildUser(id: 0, isActive: false);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    #endregion

    #region ValidateForDeactivateAsync

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var entity = BuildUser(isActive: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildUser(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildUser(id: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    #endregion

    #region ValidateForDeleteAsync

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var entity = BuildUser();
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntidadeJaDeletada_DeveRetornarErro()
    {
        var entity = BuildUser(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildUser(id: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    #endregion

    #region ValidateForRevokeAsync

    [Fact(DisplayName = "ValidateForRevokeAsync - Deve sempre retornar sucesso")]
    [Trait("Domain", "")]
    public async Task ValidateForRevokeAsync_Sempre_DeveRetornarSucesso()
    {
        var entity = BuildUser();
        var sut = CreateSut();

        var result = await sut.ValidateForRevokeAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion
}
