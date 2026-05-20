using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.Role;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.Role;

public class CreateRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private CreateRoleValidator CreateSut() => new(_localizationMock.Object);

    private static RoleEntity BuildRole(int tenantId = 1, int appId = 2, string name = "Role Test", string description = "Descrição válida")
        => new(tenantId, appId, name, description, 10);

    public CreateRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildRole());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(tenantId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando AppId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_AppIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(appId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AppId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 100 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede100Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(name: new string('A', 101)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Name exatamente 100 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExatamente100Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(name: new string('A', 100)));

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description excede 255 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionExcede255Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(description: new string('D', 256)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }
}

public class UpdateRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private UpdateRoleValidator CreateSut() => new(_localizationMock.Object);

    private static RoleEntity BuildRole(int id = 1, int tenantId = 1, string name = "Role Test", string description = "Descrição", bool isDeleted = false)
    {
        var entity = new RoleEntity(tenantId, 2, name, description, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public UpdateRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildRole());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(tenantId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class ActivateRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private ActivateRoleValidator CreateSut() => new(_localizationMock.Object);

    private static RoleEntity BuildRole(int id = 1, bool isActive = false, bool isDeleted = false)
    {
        var entity = new RoleEntity(1, 2, "Role Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public ActivateRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(isActive: false));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(isActive: false, isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class DeactivateRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeactivateRoleValidator CreateSut() => new(_localizationMock.Object);

    private static RoleEntity BuildRole(int id = 1, bool isActive = true, bool isDeleted = false)
    {
        var entity = new RoleEntity(1, 2, "Role Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeactivateRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(isActive: true));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class DeleteRoleValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeleteRoleValidator CreateSut() => new(_localizationMock.Object);

    private static RoleEntity BuildRole(int id = 1, bool isDeleted = false)
    {
        var entity = new RoleEntity(1, 2, "Role Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeleteRoleValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildRole());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildRole(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}
