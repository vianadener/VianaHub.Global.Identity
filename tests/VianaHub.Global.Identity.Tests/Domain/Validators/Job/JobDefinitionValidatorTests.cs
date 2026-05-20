using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Validators.Job;
using FluentValidation.Results;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain.Validators.Job;

public class JobDefinitionValidatorTests
{
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private JobDefinitionValidator CreateSut() => new(_localizationMock.Object);

    private static JobDefinitionEntity BuildJob(
        string category = "Categoria",
        string name = "Job Teste",
        string type = "VianaHub.Global.Identity.Infra.Job.Jobs.Cleanup.CleanupExpiredJwtKeysJob",
        bool executeOnlyOnce = false,
        string cronExpression = "0 * * * *",
        int timeoutMinutes = 5,
        int priority = 5,
        int maxRetries = 3,
        bool isSystemJob = false)
        => new(category, name, type, 10,
            cronExpression: cronExpression,
            executeOnlyOnce: executeOnlyOnce,
            timeoutMinutes: timeoutMinutes,
            priority: priority,
            maxRetries: maxRetries,
            isSystemJob: isSystemJob);

    public JobDefinitionValidatorTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("Mensagem de erro");
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("Mensagem de erro");
    }

    #region ValidateForCreateAsync

    [Fact(DisplayName = "ValidateForCreateAsync - Deve ser válido com dados corretos", Skip = "Validação de tipo via reflection requer assembly Infra.Job carregado no AppDomain - teste de integração")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildJob();
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Category é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_CategoryVazio_DeveRetornarErro()
    {
        var entity = BuildJob(category: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Category");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Name é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_NameVazio_DeveRetornarErro()
    {
        var entity = BuildJob(name: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Type é vazio")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_TypeVazio_DeveRetornarErro()
    {
        var entity = BuildJob(type: "");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Type");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando TimeoutMinutes é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_TimeoutZero_DeveRetornarErro()
    {
        var entity = BuildJob(timeoutMinutes: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TimeoutMinutes");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando Priority está fora do intervalo")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_PriorityInvalida_DeveRetornarErro()
    {
        var entity = BuildJob(priority: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Priority");
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando ExecuteOnlyOnce tem CronExpression")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_ExecuteOnlyOnceComCron_DeveRetornarErro()
    {
        var entity = BuildJob(executeOnlyOnce: true, cronExpression: "0 * * * *");
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve falhar quando recorrente não tem CronExpression")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_RecorrenteSemCron_DeveRetornarErro()
    {
        var entity = BuildJob(executeOnlyOnce: false, cronExpression: null);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.False(result.IsValid);
    }

    [Fact(DisplayName = "ValidateForCreateAsync - Deve ser válido com ExecuteOnlyOnce sem CronExpression", Skip = "Validação de tipo via reflection requer assembly Infra.Job carregado no AppDomain - teste de integração")]
    [Trait("Domain", "")]
    public async Task ValidateForCreateAsync_ExecuteOnlyOnceSemCron_DeveRetornarSucesso()
    {
        var entity = BuildJob(executeOnlyOnce: true, cronExpression: null);
        var sut = CreateSut();

        var result = await sut.ValidateForCreateAsync(entity);

        Assert.True(result.IsValid);
    }

    #endregion

    #region ValidateForUpdateAsync

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve ser válido com dados corretos", Skip = "Validação de tipo via reflection requer assembly Infra.Job carregado no AppDomain - teste de integração")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_DadosValidos_DeveRetornarSucesso()
    {
        var entity = BuildJob();
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "ValidateForUpdateAsync - Deve falhar quando TimeoutMinutes é zero")]
    [Trait("Domain", "")]
    public async Task ValidateForUpdateAsync_TimeoutZero_DeveRetornarErro()
    {
        var entity = BuildJob(timeoutMinutes: 0);
        var sut = CreateSut();

        var result = await sut.ValidateForUpdateAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TimeoutMinutes");
    }

    #endregion

    #region ValidateForDeleteAsync

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve ser válido para job não-sistema")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_JobNaoSistema_DeveRetornarSucesso()
    {
        var entity = BuildJob(isSystemJob: false);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "ValidateForDeleteAsync - Deve falhar para SystemJob")]
    [Trait("Domain", "")]
    public async Task ValidateForDeleteAsync_SystemJob_DeveRetornarErro()
    {
        var entity = BuildJob(isSystemJob: true);
        var sut = CreateSut();

        var result = await sut.ValidateForDeleteAsync(entity);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "IsSystemJob");
    }

    #endregion

    #region ValidateForDeactivateAsync

    [Fact(DisplayName = "ValidateForDeactivateAsync - Deve sempre retornar sucesso")]
    [Trait("Domain", "")]
    public async Task ValidateForDeactivateAsync_Sempre_DeveRetornarSucesso()
    {
        var entity = BuildJob();
        var sut = CreateSut();

        var result = await sut.ValidateForDeactivateAsync(entity);

        Assert.True(result.IsValid);
    }

    #endregion

    #region ValidateForRevokeAsync

    [Fact(DisplayName = "ValidateForRevokeAsync - Deve sempre retornar sucesso")]
    [Trait("Domain", "")]
    public async Task ValidateForRevokeAsync_Sempre_DeveRetornarSucesso()
    {
        var entity = BuildJob();
        var sut = CreateSut();

        var result = await sut.ValidateForRevokeAsync(entity);

        Assert.True(result.IsValid);
    }

    #endregion
}
