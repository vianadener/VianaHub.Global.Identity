using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.Job;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.Job;

public class JobDefinitionCreateValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private JobDefinitionCreateValidator CreateSut() => new(_localizationMock.Object);

    private const string ValidType = "VianaHub.Global.Identity.Infra.Job.Jobs.Cleanup.CleanupExpiredJwtKeysJob";

    private static JobDefinitionEntity BuildJob(
        string category = "Categoria",
        string name = "Job Teste",
        string type = ValidType,
        bool executeOnlyOnce = false,
        string cronExpression = "0 * * * *",
        int timeoutMinutes = 5,
        int priority = 5,
        int maxRetries = 3)
        => new(category, name, type, 10,
            cronExpression: cronExpression,
            executeOnlyOnce: executeOnlyOnce,
            timeoutMinutes: timeoutMinutes,
            priority: priority,
            maxRetries: maxRetries);

    public JobDefinitionCreateValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos", Skip = "Validação de tipo via reflection requer assembly Infra.Job carregado no AppDomain - teste de integração")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildJob());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Category é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_CategoryVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(category: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Category");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_NameVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Name excede 150 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_NameExcede150Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(name: new string('A', 151)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Type é vazio")]
    [Trait("Domain", "")]
    public async Task Validate_TypeVazio_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(type: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Type");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TimeoutMinutes é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TimeoutZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(timeoutMinutes: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TimeoutMinutes");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Priority é zero")]
    [Trait("Domain", "")]
    public async Task Validate_PriorityZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(priority: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Priority");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Priority excede 10")]
    [Trait("Domain", "")]
    public async Task Validate_PriorityExcede10_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(priority: 11));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Priority");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando MaxRetries é negativo")]
    [Trait("Domain", "")]
    public async Task Validate_MaxRetriesNegativo_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(maxRetries: -1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MaxRetries");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando ExecuteOnlyOnce tem CronExpression")]
    [Trait("Domain", "")]
    public async Task Validate_ExecuteOnlyOnceComCron_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(executeOnlyOnce: true, cronExpression: "0 * * * *"));

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando recorrente não tem CronExpression")]
    [Trait("Domain", "")]
    public async Task Validate_RecorrenteSemCron_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(executeOnlyOnce: false, cronExpression: null));

        Assert.False(result.IsValid);
    }
}

public class JobDefinitionUpdateValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private JobDefinitionUpdateValidator CreateSut() => new(_localizationMock.Object);

    private const string ValidType = "VianaHub.Global.Identity.Infra.Job.Jobs.Cleanup.CleanupExpiredJwtKeysJob";

    private static JobDefinitionEntity BuildJob(
        bool executeOnlyOnce = false,
        string cronExpression = "0 * * * *",
        int timeoutMinutes = 5,
        int priority = 5,
        int maxRetries = 3,
        string description = null)
        => new("Categoria", "Job Teste", ValidType, 10,
            description: description,
            cronExpression: cronExpression,
            executeOnlyOnce: executeOnlyOnce,
            timeoutMinutes: timeoutMinutes,
            priority: priority,
            maxRetries: maxRetries);

    public JobDefinitionUpdateValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    [Fact(DisplayName = "Validate - Deve ser válido com dados corretos", Skip = "Validação de tipo via reflection requer assembly Infra.Job carregado no AppDomain - teste de integração")]
    [Trait("Domain", "")]
    public async Task Validate_DadosValidos_DeveRetornarSucesso()
    {
        var result = await CreateSut().ValidateAsync(BuildJob());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando TimeoutMinutes é zero")]
    [Trait("Domain", "")]
    public async Task Validate_TimeoutZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(timeoutMinutes: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TimeoutMinutes");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Priority é zero")]
    [Trait("Domain", "")]
    public async Task Validate_PriorityZero_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(priority: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Priority");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando MaxRetries é negativo")]
    [Trait("Domain", "")]
    public async Task Validate_MaxRetriesNegativo_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(maxRetries: -1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "MaxRetries");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando Description excede 1000 caracteres")]
    [Trait("Domain", "")]
    public async Task Validate_DescriptionExcede1000Chars_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(description: new string('D', 1001)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact(DisplayName = "Validate - Deve falhar quando ExecuteOnlyOnce tem CronExpression")]
    [Trait("Domain", "")]
    public async Task Validate_ExecuteOnlyOnceComCron_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(executeOnlyOnce: true, cronExpression: "0 * * * *"));

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "Validate - Deve falhar quando recorrente não tem CronExpression")]
    [Trait("Domain", "")]
    public async Task Validate_RecorrenteSemCron_DeveRetornarErro()
    {
        var result = await CreateSut().ValidateAsync(BuildJob(executeOnlyOnce: false, cronExpression: null));

        Assert.False(result.IsValid);
    }
}
