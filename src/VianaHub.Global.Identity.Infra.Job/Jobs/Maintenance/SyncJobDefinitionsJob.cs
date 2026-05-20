using VianaHub.Global.Identity.Infra.Job.Interfaces;
using Microsoft.Extensions.Logging;

namespace VianaHub.Global.Identity.Infra.Job.Jobs.Maintenance;

/// <summary>
/// Job responsável por disparar a sincronização de JobDefinitions com o Hangfire.
/// Agendado via JobDefinition (JobType = full name desta classe). CronExpression controla periodicidade;
/// IsActive controla ativação/desativação.
/// </summary>
public class SyncJobDefinitionsJob : IJob
{
    private readonly IJobSyncService _jobSyncService;
    private readonly ILogger<SyncJobDefinitionsJob> _logger;

    public SyncJobDefinitionsJob(IJobSyncService jobSyncService, ILogger<SyncJobDefinitionsJob> logger)
    {
        _jobSyncService = jobSyncService;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SyncJobDefinitionsJob] Starting job to synchronize JobDefinitions with Hangfire");

        try
        {
            await _jobSyncService.SyncJobsWithHangfire(cancellationToken);
            _logger.LogInformation("[SyncJobDefinitionsJob] Synchronization completed successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("[SyncJobDefinitionsJob] Synchronization was cancelled");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "[SyncJobDefinitionsJob] Error while synchronizing JobDefinitions with Hangfire");
            throw; // rethrow to let Hangfire handle retries if configured
        }
    }
}
