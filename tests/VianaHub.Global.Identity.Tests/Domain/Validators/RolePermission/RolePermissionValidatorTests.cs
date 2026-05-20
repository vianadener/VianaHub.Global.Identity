using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.RolePermission;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.RolePermission;

public class RolePermissionValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private RolePermissionValidator CreateSut() => new(_localizationMock.Object);

    private static RolePermissionEntity BuildRolePermission(int tenantId = 1, int appId = 2, int roleId = 3, int resourceId = 4, int actionId = 5)
        => new(tenantId, appId, roleId, resourceId, actionId);

    public RolePermissionValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    #region Validate

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildRolePermission();
        var sut = CreateSut();

        var result = await sut.ValidateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildRolePermission(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando RoleId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_RoleIdZero_DeveRetornarErro()
    {
        var entity = BuildRolePermission(roleId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RoleId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando ResourceId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_ResourceIdZero_DeveRetornarErro()
    {
        var entity = BuildRolePermission(resourceId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ResourceId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando ActionId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_ActionIdZero_DeveRetornarErro()
    {
        var entity = BuildRolePermission(actionId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ActionId");
    }

    #endregion
}
