using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.UserRole;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.UserRole;

public class UserRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private UserRoleValidator CreateSut() => new(_localizationMock.Object);

    private static UserRoleEntity BuildUserRole(int tenantId = 1, int appId = 2, int userId = 10, int roleId = 5)
        => new(tenantId, appId, userId, roleId);

    public UserRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    #region ValidateForCreateAsync

    [Fact(DisplayName = "ValidateForCreateAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildUserRole();
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando AppId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_AppIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(appId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppId");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando UserId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_UserIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(userId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando RoleId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_RoleIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(roleId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RoleId");
    }

    #endregion

    #region ValidateForDeleteAsync

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildUserRole();
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_TenantIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(tenantId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando AppId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_AppIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(appId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppId");
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando UserId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_UserIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(userId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserId");
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar quando RoleId é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_RoleIdZero_DeveRetornarErro()
    {
        var entity = BuildUserRole(roleId: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RoleId");
    }

    #endregion

    #region ValidateForUpdateAsync / ValidateForActivateAsync / ValidateForDeactivateAsync / ValidateForRevokeAsync

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve lançar NotImplementedException")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_DeveLancarNotImplementedException()
    {
        var entity = BuildUserRole();
        var sut = CreateSut();

        await Assert.ThrowsAsync<NotImplementedException>(() => sut.ValidateForUpdateAsync(entity));
    }

    [Fact(DisplayName = "ValidateForActivateAsync - Deve lançar NotImplementedException")]
    [Trait("Domain", "")]
    public async Task ValidateForActivateAsync_DeveLancarNotImplementedException()
    {
        var entity = BuildUserRole();
        var sut = CreateSut();

        await Assert.ThrowsAsync<NotImplementedException>(() => sut.ValidateForActivateAsync(entity));
    }

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve lançar NotImplementedException")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_DeveLancarNotImplementedException()
    {
        var entity = BuildUserRole();
        var sut = CreateSut();

        await Assert.ThrowsAsync<NotImplementedException>(() => sut.ValidateForDeactivateAsync(entity));
    }

    [Fact(DisplayName = "ValidateForRevokeAsync - Deve lançar NotImplementedException")]
    [Trait("Domain", "")]
    public async Task ValidateForRevokeAsync_DeveLancarNotImplementedException()
    {
        var entity = BuildUserRole();
        var sut = CreateSut();

        await Assert.ThrowsAsync<NotImplementedException>(() => sut.ValidateForRevokeAsync(entity));
    }

    #endregion
}
