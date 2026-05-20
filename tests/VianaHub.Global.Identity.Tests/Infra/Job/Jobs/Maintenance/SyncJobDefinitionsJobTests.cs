using VianaHub.Global.Identity.Infra.Job.Interfaces;
using VianaHub.Global.Identity.Infra.Job.Jobs.Maintenance;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.Jobs.Maintenance;

public class SyncJobDefinitionsJobTests
{
    private readonly Mock<IJobSyncService> _jobSyncServiceMock = new();
    private readonly Mock<ILogger<SyncJobDefinitionsJob>> _loggerMock = new();

    private SyncJobDefinitionsJob CreateSut() =>
        new(_jobSyncServiceMock.Object, _loggerMock.Object);

    #region Execute

    [Fact(DisplayName = "Execute - Deve sincronizar jobs com sucesso")]
    [Trait("Infra.Job", "")]
    public async Task Execute_Sucesso_DeveChamarSyncJobsWithHangfire()
    {
        _jobSyncServiceMock
            .Setup(x => x.SyncJobsWithHangfire(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.Execute(CancellationToken.None);

        _jobSyncServiceMock.Verify(x => x.SyncJobsWithHangfire(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Execute - Deve propagar exceção quando sincronização falha")]
    [Trait("Infra.Job", "")]
    public async Task Execute_ExcecaoNoSync_DevePropagarExcecao()
    {
        _jobSyncServiceMock
            .Setup(x => x.SyncJobsWithHangfire(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha no Hangfire"));

        var sut = CreateSut();
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Execute(CancellationToken.None));
    }

    [Fact(DisplayName = "Execute - Deve capturar OperationCanceledException sem propagar")]
    [Trait("Infra.Job", "")]
    public async Task Execute_OperacaoCancelada_NaoDevePropagar()
    {
        _jobSyncServiceMock
            .Setup(x => x.SyncJobsWithHangfire(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.Execute(CancellationToken.None));

        Assert.Null(exception);
    }

    #endregion
}
