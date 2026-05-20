using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Job.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Services;

public class HangfireJobServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<HangfireJobService>> _loggerMock = new();
    private readonly Mock<IJobExecutor> _jobExecutorMock = new();

    private HangfireJobService CreateSut() =>
        new(_serviceProviderMock.Object, _loggerMock.Object, _jobExecutorMock.Object);

    private static JobDefinitionEntity BuildJobDefinition(
        string name = "TestJob",
        string cronExpression = "0 3 * * *",
        string timeZoneId = "UTC",
        bool executeOnlyOnce = false)
    {
        return new JobDefinitionEntity(
            jobCategory: "Test",
            jobName: name,
            jobType: "TestType",
            createdBy: 1,
            cronExpression: cronExpression,
            timeZoneId: timeZoneId,
            executeOnlyOnce: executeOnlyOnce);
    }

    #region RegisterRecurringAsync

    [Fact(DisplayName = "RegisterRecurringAsync - Deve registrar job recorrente com sucesso")]
    [Trait("Infra.Job", "")]
    public async Task RegisterRecurringAsync_Sucesso_DeveRegistrarViaExecutor()
    {
        var jobDef = BuildJobDefinition();
        _jobExecutorMock.Setup(x => x.RegisterRecurringJobAsync(jobDef)).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.RegisterRecurringAsync(jobDef);

        _jobExecutorMock.Verify(x => x.RegisterRecurringJobAsync(jobDef), Times.Once);
    }

    [Fact(DisplayName = "RegisterRecurringAsync - Deve lançar ArgumentNullException quando jobDef for null")]
    [Trait("Infra.Job", "")]
    public async Task RegisterRecurringAsync_JobDefNull_DeveLancarArgumentNullException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.RegisterRecurringAsync(null!));
    }

    [Fact(DisplayName = "RegisterRecurringAsync - Deve ignorar job ExecuteOnlyOnce")]
    [Trait("Infra.Job", "")]
    public async Task RegisterRecurringAsync_ExecuteOnlyOnce_NaoDeveRegistrar()
    {
        var jobDef = BuildJobDefinition(executeOnlyOnce: true);

        var sut = CreateSut();
        await sut.RegisterRecurringAsync(jobDef);

        _jobExecutorMock.Verify(x => x.RegisterRecurringJobAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "RegisterRecurringAsync - Deve ignorar job sem CronExpression")]
    [Trait("Infra.Job", "")]
    public async Task RegisterRecurringAsync_SemCronExpression_NaoDeveRegistrar()
    {
        var jobDef = BuildJobDefinition(cronExpression: null!);

        var sut = CreateSut();
        await sut.RegisterRecurringAsync(jobDef);

        _jobExecutorMock.Verify(x => x.RegisterRecurringJobAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "RegisterRecurringAsync - Deve usar UTC quando TimeZoneId é inválido")]
    [Trait("Infra.Job", "")]
    public async Task RegisterRecurringAsync_TimeZoneIdInvalido_DeveUsarUtc()
    {
        var jobDef = BuildJobDefinition(timeZoneId: "TimeZone/Invalida");
        _jobExecutorMock.Setup(x => x.RegisterRecurringJobAsync(jobDef)).ReturnsAsync(true);

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.RegisterRecurringAsync(jobDef));

        Assert.Null(exception);
        _jobExecutorMock.Verify(x => x.RegisterRecurringJobAsync(jobDef), Times.Once);
    }

    #endregion

    #region RemoveRecurringAsync

    [Fact(DisplayName = "RemoveRecurringAsync - Deve remover job com sucesso")]
    [Trait("Infra.Job", "")]
    public async Task RemoveRecurringAsync_Sucesso_DeveRemoverViaExecutor()
    {
        _jobExecutorMock.Setup(x => x.RemoveRecurringJobAsync("TestJob")).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.RemoveRecurringAsync("TestJob");

        _jobExecutorMock.Verify(x => x.RemoveRecurringJobAsync("TestJob"), Times.Once);
    }

    #endregion

    #region EnqueueJobAsync

    [Fact(DisplayName = "EnqueueJobAsync - Deve enfileirar job e retornar id")]
    [Trait("Infra.Job", "")]
    public async Task EnqueueJobAsync_Sucesso_DeveRetornarJobId()
    {
        var jobDef = BuildJobDefinition();
        _jobExecutorMock.Setup(x => x.EnqueueJobAsync(jobDef)).ReturnsAsync("hangfire-id-123");

        var sut = CreateSut();
        var result = await sut.EnqueueJobAsync(jobDef);

        Assert.Equal("hangfire-id-123", result);
        _jobExecutorMock.Verify(x => x.EnqueueJobAsync(jobDef), Times.Once);
    }

    [Fact(DisplayName = "EnqueueJobAsync - Deve lançar ArgumentNullException quando jobDef for null")]
    [Trait("Infra.Job", "")]
    public async Task EnqueueJobAsync_JobDefNull_DeveLancarArgumentNullException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.EnqueueJobAsync(null!));
    }

    #endregion
}
