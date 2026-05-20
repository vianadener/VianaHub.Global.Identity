using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Job.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Services;

public class JobSyncServiceTests
{
    private readonly Mock<IJobDefinitionDataRepository> _repoMock = new();
    private readonly Mock<IJobSchedulerService> _schedulerMock = new();
    private readonly Mock<ILogger<JobSyncService>> _loggerMock = new();

    private JobSyncService CreateSut() =>
        new(_repoMock.Object, _schedulerMock.Object, _loggerMock.Object);

    private static JobDefinitionEntity BuildJob(
        string name = "Job1",
        bool isActive = true,
        bool isDeleted = false,
        bool executeOnlyOnce = false)
    {
        var job = new JobDefinitionEntity(
            jobCategory: "Test",
            jobName: name,
            jobType: "TestType",
            createdBy: 1,
            cronExpression: "0 * * * *",
            executeOnlyOnce: executeOnlyOnce);

        if (!isActive) job.Deactivate(1);
        if (isDeleted) job.Delete(1);
        return job;
    }

    #region SyncJobsWithHangfire

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve registrar jobs ativos no Hangfire")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_Sucesso_DeveRegistrarJobsAtivos()
    {
        var jobs = new List<JobDefinitionEntity> { BuildJob("Job1"), BuildJob("Job2") };
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);
        _schedulerMock.Setup(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.SyncJobsWithHangfire(CancellationToken.None);

        _schedulerMock.Verify(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve ignorar jobs inativos")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_JobInativo_DeveIgnorar()
    {
        var jobs = new List<JobDefinitionEntity> { BuildJob("Job1", isActive: false) };
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);

        var sut = CreateSut();
        await sut.SyncJobsWithHangfire(CancellationToken.None);

        _schedulerMock.Verify(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve ignorar jobs excluídos")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_JobExcluido_DeveIgnorar()
    {
        var jobs = new List<JobDefinitionEntity> { BuildJob("Job1", isDeleted: true) };
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);

        var sut = CreateSut();
        await sut.SyncJobsWithHangfire(CancellationToken.None);

        _schedulerMock.Verify(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve ignorar jobs ExecuteOnlyOnce")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_ExecuteOnlyOnce_DeveIgnorar()
    {
        var jobs = new List<JobDefinitionEntity> { BuildJob("Job1", executeOnlyOnce: true) };
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);

        var sut = CreateSut();
        await sut.SyncJobsWithHangfire(CancellationToken.None);

        _schedulerMock.Verify(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve continuar sync quando um job falha ao registrar")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_FalhaAoRegistrarUmJob_DeveContinuarParaOutros()
    {
        var jobs = new List<JobDefinitionEntity> { BuildJob("Job1"), BuildJob("Job2") };
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);
        _schedulerMock.SetupSequence(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()))
            .ThrowsAsync(new Exception("Falha Job1"))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.SyncJobsWithHangfire(CancellationToken.None));

        Assert.Null(exception);
        _schedulerMock.Verify(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve completar sem erros quando não há jobs")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_SemJobs_DeveCompletarSemErros()
    {
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.SyncJobsWithHangfire(CancellationToken.None));

        Assert.Null(exception);
        _schedulerMock.Verify(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve respeitar cancelamento e não registrar mais jobs")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_TokenCancelado_DeveParar()
    {
        var cts = new CancellationTokenSource();
        var jobs = new List<JobDefinitionEntity> { BuildJob("Job1"), BuildJob("Job2") };

        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);
        _schedulerMock.Setup(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()))
            .Callback(() => cts.Cancel())
            .Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.SyncJobsWithHangfire(cts.Token));

        Assert.Null(exception);
        _schedulerMock.Verify(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>()), Times.Once);
    }

    #endregion

    #region ExecuteJob

    [Fact(DisplayName = "ExecuteJob - Deve completar sem erros")]
    [Trait("Infra.Job", "")]
    public async Task ExecuteJob_Sucesso_DeveCompletarSemErros()
    {
        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.ExecuteJob("SomeType", "SomeMethod"));

        Assert.Null(exception);
    }

    #endregion
}
