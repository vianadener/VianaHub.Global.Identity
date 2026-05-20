using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.Tenant;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.Tenant;

public class CreateTenantValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private CreateTenantValidator CreateSut() => new(_localizationMock.Object);

    private static TenantEntity BuildTenant(string name = "Tenant Test", string description = "Descrição válida", string alias = "TTT")
        => new(name, description, alias, null, null, null, 10);

    public CreateTenantValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 200 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede200Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(name: new string('A', 201)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Name exatamente 200 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExatamente200Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(name: new string('A', 200)));

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description excede 500 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionExcede500Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(description: new string('D', 501)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Alias é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_AliasVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(alias: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Alias");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Alias excede 30 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_AliasExcede30Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(alias: new string('A', 31)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Alias");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Alias exatamente 30 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_AliasExatamente30Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(alias: new string('A', 30)));

        Assert.True(result.IsValid);
    }
}

public class UpdateTenantValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private UpdateTenantValidator CreateSut() => new(_localizationMock.Object);

    private static TenantEntity BuildTenant(int id = 1, string name = "Tenant Test", string description = "Descrição", string alias = "TTT", bool isDeleted = false)
    {
        var entity = new TenantEntity(name, description, alias, null, null, null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public UpdateTenantValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Alias é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_AliasVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(alias: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Alias");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class ActivateTenantValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private ActivateTenantValidator CreateSut() => new(_localizationMock.Object);

    private static TenantEntity BuildTenant(int id = 1, bool isActive = false, bool isDeleted = false)
    {
        var entity = new TenantEntity("Tenant Test", "Descrição", "TTT", null, null, null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public ActivateTenantValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(isActive: false));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(isActive: false, isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class DeactivateTenantValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeactivateTenantValidator CreateSut() => new(_localizationMock.Object);

    private static TenantEntity BuildTenant(int id = 1, bool isActive = true, bool isDeleted = false)
    {
        var entity = new TenantEntity("Tenant Test", "Descrição", "TTT", null, null, null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeactivateTenantValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(isActive: true));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class DeleteTenantValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeleteTenantValidator CreateSut() => new(_localizationMock.Object);

    private static TenantEntity BuildTenant(int id = 1, bool isDeleted = false)
    {
        var entity = new TenantEntity("Tenant Test", "Descrição", "TTT", null, null, null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeleteTenantValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildTenant(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}
