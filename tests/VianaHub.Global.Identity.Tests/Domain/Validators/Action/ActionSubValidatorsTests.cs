using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.Action;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.Action;

public class CreateActionValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private CreateActionValidator CreateSut() => new(_localizationMock.Object);

    private static ActionEntity BuildAction(string name = "Action Test", string description = "Descrição válida")
        => new(1, 2, name, description, 10);

    public CreateActionValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildAction());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 50 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede50Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(name: new string('A', 51)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Name exatamente 50 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExatamente50Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(name: new string('A', 50)));

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description excede 255 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionExcede255Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(description: new string('D', 256)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Description exatamente 255 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionExatamente255Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(description: new string('D', 255)));

        Assert.True(result.IsValid);
    }
}

public class UpdateActionValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private UpdateActionValidator CreateSut() => new(_localizationMock.Object);

    private static ActionEntity BuildAction(int id = 1, string name = "Action Test", string description = "Descrição", bool isDeleted = false)
    {
        var entity = new ActionEntity(1, 2, name, description, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public UpdateActionValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildAction());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 50 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede50Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(name: new string('A', 51)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(description: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class ActivateActionValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private ActivateActionValidator CreateSut() => new(_localizationMock.Object);

    private static ActionEntity BuildAction(int id = 1, bool isActive = false, bool isDeleted = false)
    {
        var entity = new ActionEntity(1, 2, "Action Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public ActivateActionValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade inativa e não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeInativaNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(isActive: false));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(isActive: false, isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está ativa")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaAtiva_DeveRetornarErro()
    {
        var entity = new ActionEntity(1, 2, "Action Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, 1);

        var result = await CreateSut().ValidateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsActive");
    }
}

public class DeactivateActionValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeactivateActionValidator CreateSut() => new(_localizationMock.Object);

    private static ActionEntity BuildAction(int id = 1, bool isActive = true, bool isDeleted = false)
    {
        var entity = new ActionEntity(1, 2, "Action Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeactivateActionValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade ativa e não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeAtivaNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(isActive: true));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está inativa")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaInativa_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(isActive: false));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsActive");
    }
}

public class DeleteActionValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeleteActionValidator CreateSut() => new(_localizationMock.Object);

    private static ActionEntity BuildAction(int id = 1, bool isDeleted = false)
    {
        var entity = new ActionEntity(1, 2, "Action Test", "Descrição", 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeleteActionValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildAction());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildAction(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}
