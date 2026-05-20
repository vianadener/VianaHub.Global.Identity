using VianaHub.Global.Identity.Infra.Job.Interfaces;
using VianaHub.Global.Identity.Infra.Job.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Services;

public class NoOpJobSyncServiceTests
{
    private readonly Mock<ILogger<NoOpJobSyncService>> _loggerMock = new();

    private NoOpJobSyncService CreateSut() => new(_loggerMock.Object);

    #region SyncJobsWithHangfire

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve completar sem executar nenhuma ação")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_Sucesso_DeveCompletarSemAcao()
    {
        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.SyncJobsWithHangfire(CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "SyncJobsWithHangfire - Deve completar mesmo com token cancelado")]
    [Trait("Infra.Job", "")]
    public async Task SyncJobsWithHangfire_TokenCancelado_DeveCompletarSemErro()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.SyncJobsWithHangfire(cts.Token));

        Assert.Null(exception);
    }

    #endregion

    #region ExecuteJob

    [Fact(DisplayName = "ExecuteJob - Deve completar sem executar nenhuma ação")]
    [Trait("Infra.Job", "")]
    public async Task ExecuteJob_Sucesso_DeveCompletarSemAcao()
    {
        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.ExecuteJob("SomeType", "SomeMethod"));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "ExecuteJob - Deve aceitar qualquer jobType e jobMethod")]
    [Trait("Infra.Job", "")]
    public async Task ExecuteJob_QualquerParametro_DeveCompletarSemErro()
    {
        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.ExecuteJob(string.Empty, string.Empty));

        Assert.Null(exception);
    }

    #endregion
}
