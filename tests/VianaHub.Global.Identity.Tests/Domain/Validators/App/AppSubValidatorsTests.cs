using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.App;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.App;

public class CreateAppValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private CreateAppValidator CreateSut() => new(_localizationMock.Object);

    private static AppEntity BuildApp(int tenantId = 1, string name = "App Test", string description = "Descrição válida")
        => new(tenantId, name, description, 10);

    public CreateAppValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildApp());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(tenantId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 200 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede200Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(name: new string('A', 201)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Name exatamente 200 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExatamente200Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(name: new string('A', 200)));

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description excede 500 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionExcede500Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(description: new string('D', 501)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Description exatamente 500 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionExatamente500Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(description: new string('D', 500)));

        Assert.True(result.IsValid);
    }
}

public class UpdateAppValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private UpdateAppValidator CreateSut() => new(_localizationMock.Object);

    private static AppEntity BuildApp(int id = 1, int tenantId = 1, string name = "App Test", string description = "Descrição")
    {
        var entity = new AppEntity(tenantId, name, description, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    public UpdateAppValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildApp());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(tenantId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }
}

public class ActivateAppValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private ActivateAppValidator CreateSut() => new(_localizationMock.Object);

    private static AppEntity BuildApp(int id = 1, bool isActive = false, bool isDeleted = false)
    {
        var entity = new AppEntity(1, "App Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public ActivateAppValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade inativa não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeInativaNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(isActive: false));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(isActive: false, isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está ativa")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaAtiva_DeveRetornarErro()
    {
        var entity = new AppEntity(1, "App Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, 1);

        var result = await CreateSut().ValidateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsActive");
    }
}

public class DeactivateAppValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeactivateAppValidator CreateSut() => new(_localizationMock.Object);

    private static AppEntity BuildApp(int id = 1, bool isActive = true, bool isDeleted = false)
    {
        var entity = new AppEntity(1, "App Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeactivateAppValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade ativa não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeAtivaNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(isActive: true));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está inativa")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaInativa_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(isActive: false));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsActive");
    }
}

public class DeleteAppValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeleteAppValidator CreateSut() => new(_localizationMock.Object);

    private static AppEntity BuildApp(int id = 1, bool isDeleted = false)
    {
        var entity = new AppEntity(1, "App Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeleteAppValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildApp());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildApp(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}
