using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.Tenant;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.Tenant;

public class TenantValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private TenantValidator CreateSut() => new(_localizationMock.Object);

    private static TenantEntity BuildTenant(int id = 1, string name = "Tenant Test", string description = "Descrição", string alias = "TTT", bool isActive = true, bool isDeleted = false)
    {
        var entity = new TenantEntity(name, description, alias, null, null, null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive)
            entity.Deactivate(10);
        if (isDeleted)
            entity.Delete(10);
        return entity;
    }

    public TenantValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    #region ValidateForCreateAsync

    [Fact(DisplayName = "ValidateForCreateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildTenant();
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_NameVazio_DeveRetornarErro()
    {
        var entity = BuildTenant(name: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Name excede 200 caracteres")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_NameExcede200Chars_DeveRetornarErro()
    {
        var entity = BuildTenant(name: new string('A', 201));
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DescriptionVazio_DeveRetornarErro()
    {
        var entity = BuildTenant(description: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Description excede 500 caracteres")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DescriptionExcede500Chars_DeveRetornarErro()
    {
        var entity = BuildTenant(description: new string('D', 501));
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Alias é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_AliasVazio_DeveRetornarErro()
    {
        var entity = BuildTenant(alias: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Alias");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Alias excede 30 caracteres")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_AliasExcede30Chars_DeveRetornarErro()
    {
        var entity = BuildTenant(alias: new string('A', 31));
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Alias");
    }

    #endregion

    #region ValidateForUpdateAsync

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildTenant();
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildTenant(id: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildTenant(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    #endregion

    #region ValidateForActivateAsync

    [Fact(DisplayName = "ValidateForActivateAsync - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var entity = BuildTenant(isActive: false);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildTenant(isActive: false, isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForActivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildTenant(id: 0, isActive: false);
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
        var entity = BuildTenant(isActive: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_EntidadeDeletada_DeveRetornarErro()
    {
        var entity = BuildTenant(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildTenant(id: 0);
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
        var entity = BuildTenant();
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_EntidadeJaDeletada_DeveRetornarErro()
    {
        var entity = BuildTenant(isDeleted: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_IdZero_DeveRetornarErro()
    {
        var entity = BuildTenant(id: 0);
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
        var entity = BuildTenant();
        var sut = CreateSut();

        var result = await sut.ValidateForRevokeAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion
}
