using VianaHub.Global.Identity.Infra.Job.HostedServices;
using VianaHub.Global.Identity.Infra.Job.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Job.HostedServices;

public class JobSyncHostedServiceTests
{
    private readonly Mock<IJobSyncService> _jobSyncServiceMock = new();
    private readonly Mock<ILogger<JobSyncHostedService>> _loggerMock = new();

    private JobSyncHostedService CreateSut()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_jobSyncServiceMock.Object);

        var serviceProvider = services.BuildServiceProvider();
        return new JobSyncHostedService(serviceProvider, _loggerMock.Object);
    }

    #region StartAsync

    [Fact(DisplayName = "StartAsync - Deve sincronizar jobs com sucesso na inicialização")]
    [Trait("Infra.Job", "")]
    public async Task StartAsync_Sucesso_DeveChamarSyncJobsWithHangfire()
    {
        _jobSyncServiceMock
            .Setup(x => x.SyncJobsWithHangfire(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        await sut.StartAsync(CancellationToken.None);

        _jobSyncServiceMock.Verify(x => x.SyncJobsWithHangfire(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "StartAsync - Deve capturar exceção e não propagar erro")]
    [Trait("Infra.Job", "")]
    public async Task StartAsync_ExcecaoNoSync_NaoDevePropagar()
    {
        _jobSyncServiceMock
            .Setup(x => x.SyncJobsWithHangfire(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Erro de sincronização"));

        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.StartAsync(CancellationToken.None));

        Assert.Null(exception);
    }

    #endregion

    #region StopAsync

    [Fact(DisplayName = "StopAsync - Deve completar sem erros")]
    [Trait("Infra.Job", "")]
    public async Task StopAsync_Sucesso_DeveCompletarSemErros()
    {
        var sut = CreateSut();
        var exception = await Record.ExceptionAsync(() => sut.StopAsync(CancellationToken.None));

        Assert.Null(exception);
    }

    #endregion
}
