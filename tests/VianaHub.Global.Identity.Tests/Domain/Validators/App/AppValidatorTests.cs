using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.App;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.App;

public class AppValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private AppValidator CreateSut() => new(_localizationMock.Object);

    private static AppEntity BuildApp(int id = 1, int tenantId = 1, string name = "App Test", string description = "Descrição", bool isActive = true, bool isDeleted = false)
    {
        var entity = new AppEntity(tenantId, name, description, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive)
            entity.Deactivate(10);
        if (isDeleted)
            entity.Delete(10);
        return entity;
    }

    public AppValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    #region ValidateForCreateAsync

    [Fact(DisplayName = "ValidateForCreateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildApp();
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildApp(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_NameVazio_DeveRetornarErro()
    {
        var entity = BuildApp(name: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Name excede 200 caracteres")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_NameExcede200Chars_DeveRetornarErro()
    {
        var entity = BuildApp(name: new string('A', 201));
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DescriptionVazio_DeveRetornarErro()
    {
        var entity = BuildApp(description: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Description excede 500 caracteres")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DescriptionExcede500Chars_DeveRetornarErro()
    {
        var entity = BuildApp(description: new string('D', 501));
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    #endregion

    #region ValidateForUpdateAsync

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildApp();
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildApp(id: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildApp(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    #endregion

    #region ValidateForActivateAsync

    [Fact(DisplayName = "ValidateForActivateAsync - Deve ser válido com entidade inativa não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeInativa_DeveRetornarSucesso()
    {
        var entity = BuildApp(isActive: false);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando entidade já está ativa")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeJaAtiva_DeveRetornarErro()
    {
        var entity = BuildApp(isActive: true);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsActive");
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildApp(isActive: false, isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    #endregion

    #region ValidateForDeactivateAsync

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve ser válido com entidade ativa não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_EntidadeAtiva_DeveRetornarSucesso()
    {
        var entity = BuildApp(isActive: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve falhar quando entidade já está inativa")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_EntidadeJaInativa_DeveRetornarErro()
    {
        var entity = BuildApp(isActive: false);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsActive");
    }

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildApp(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    #endregion

    #region ValidateForDeleteAsync

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var entity = BuildApp();
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntidadeJaDeletada_DeveRetornarErro()
    {
        var entity = BuildApp(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildApp(id: 0);
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
        var entity = BuildApp();
        var sut = CreateSut();

        var result = await sut.ValidateForRevokeAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion
}
