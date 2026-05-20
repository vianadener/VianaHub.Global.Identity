using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.UserRole;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.UserRole;

public class CreateUserRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private CreateUserRoleValidator CreateSut() => new(_localizationMock.Object);

    private static UserRoleEntity BuildUserRole(int tenantId = 1, int appId = 2, int userId = 10, int roleId = 5)
        => new(tenantId, appId, userId, roleId);

    public CreateUserRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(tenantId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando AppId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_AppIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(appId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando UserId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_UserIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(userId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando RoleId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_RoleIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(roleId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RoleId");
    }
}

public class DeleteUserRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeleteUserRoleValidator CreateSut() => new(_localizationMock.Object);

    private static UserRoleEntity BuildUserRole(int tenantId = 1, int appId = 2, int userId = 10, int roleId = 5)
        => new(tenantId, appId, userId, roleId);

    public DeleteUserRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(tenantId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando AppId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_AppIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(appId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando UserId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_UserIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(userId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando RoleId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_RoleIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUserRole(roleId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RoleId");
    }
}
