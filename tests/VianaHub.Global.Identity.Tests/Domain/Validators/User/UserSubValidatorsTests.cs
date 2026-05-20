using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.User;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.User;

public class CreateUserValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private CreateUserValidator CreateSut() => new(_localizationMock.Object);

    private const string ValidHash = "hashedpassword_at_least_sixty_chars_XXXXXXXXXXXXXXXXXXXXXXXXXX";

    private static UserEntity BuildUser(int tenantId = 1, string name = "Usuário Teste", string passwordHash = ValidHash)
        => new(tenantId, name, "login@test.com", passwordHash, null, 10);

    public CreateUserValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUser());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TenantId é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TenantIdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(tenantId: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TenantId");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 150 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede150Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(name: new string('A', 151)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com Name exatamente 150 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExatamente150Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(name: new string('A', 150)));

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando PasswordHash é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_PasswordHashVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(passwordHash: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando PasswordHash tem menos de 60 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_PasswordHashMenorQue60Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(passwordHash: new string('x', 59)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "PasswordHash");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com PasswordHash de exatamente 60 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_PasswordHashExatamente60Chars_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(passwordHash: new string('x', 60)));

        Assert.True(result.IsValid);
    }
}

public class UpdateUserValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private UpdateUserValidator CreateSut() => new(_localizationMock.Object);

    private static UserEntity BuildUser(int id = 1, string name = "Usuário Teste", bool isDeleted = false)
    {
        var entity = new UserEntity(1, name, "login@test.com", "hashedpassword_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public UpdateUserValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUser());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 150 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede150Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(name: new string('A', 151)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }
}

public class ActivateUserValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private ActivateUserValidator CreateSut() => new(_localizationMock.Object);

    private static UserEntity BuildUser(int id = 1, bool isActive = false, bool isDeleted = false)
    {
        var entity = new UserEntity(1, "Usuário Teste", "login@test.com", "hashedpassword_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public ActivateUserValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(isActive: false));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(isActive: false, isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class DeactivateUserValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeactivateUserValidator CreateSut() => new(_localizationMock.Object);

    private static UserEntity BuildUser(int id = 1, bool isActive = true, bool isDeleted = false)
    {
        var entity = new UserEntity(1, "Usuário Teste", "login@test.com", "hashedpassword_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (!isActive) entity.Deactivate(10);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeactivateUserValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(isActive: true));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}

public class DeleteUserValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private DeleteUserValidator CreateSut() => new(_localizationMock.Object);

    private static UserEntity BuildUser(int id = 1, bool isDeleted = false)
    {
        var entity = new UserEntity(1, "Usuário Teste", "login@test.com", "hashedpassword_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", null, 10);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (isDeleted) entity.Delete(10);
        return entity;
    }

    public DeleteUserValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com entidade não deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeNaoDeletada_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildUser());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Id é zero")]
    [Trait("Domain", "")]
    public async Task Validate_IdZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(id: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando entidade já está deletada")]
    [Trait("Domain", "")]
    public async Task Validate_EntidadeJaDeletada_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildUser(isDeleted: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsDeleted");
    }
}
