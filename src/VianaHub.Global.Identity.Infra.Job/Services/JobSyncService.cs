using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Job.Interfaces;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace VianaHub.Global.Identity.Infra.Job.Services;

public class JobSyncService : IJobSyncService
{
    private readonly IJobDefinitionDataRepository _repo;
    private readonly IJobSchedulerService _scheduler;
    private readonly ILogger<JobSyncService> _logger;

    public JobSyncService(
        IJobDefinitionDataRepository repo,
        IJobSchedulerService scheduler,
        ILogger<JobSyncService> logger)
    {
        _repo = repo;
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task SyncJobsWithHangfire(CancellationToken cancellationToken = default)
    {
        var traceId = Guid.NewGuid().ToString("N");
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["Component"] = "JobSyncService"
        }))
        {
            _logger.LogInformation("[{Component}] [{TraceId}] starting synchronization of jobs with Hangfire", "JobSyncService", traceId);

            try
            {
                var allJobs = await _repo.GetAllAsync(cancellationToken);
                var activeRecurring = allJobs.Where(j => j != null && j.IsActive && !j.IsDeleted && !j.ExecuteOnlyOnce).ToList();

                _logger.LogInformation("[{Component}] [{TraceId}] found {Count} active recurring jobs to sync", "JobSyncService", traceId, activeRecurring.Count);

                var registered = 0;
                foreach (var job in activeRecurring)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        await _scheduler.RegisterRecurringAsync(job);
                        job.SetHangfireRegistration(job.Name);
                        await _repo.UpdateAsync(job, cancellationToken);
                        registered++;
                        _logger.LogInformation("[{Component}] [{TraceId}] registered job '{JobName}' in Hangfire", "JobSyncService", traceId, job.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{Component}] [{TraceId}] failed to register job '{JobName}'", "JobSyncService", traceId, job.Name);
                    }
                }

                // Remoção de jobs órfãos no Hangfire: aqueles registrados no Hangfire mas inexistentes no DB
                var removed = 0;
                try
                {
                    if (JobStorage.Current is null)
                    {
                        _logger.LogWarning("[{Component}] [{TraceId}] Hangfire JobStorage.Current is null - skipping orphan removal", "JobSyncService", traceId);
                    }
                    else
                    {
                        var connection = JobStorage.Current.GetConnection();
                        var recurringJobs = connection.GetRecurringJobs() ?? Enumerable.Empty<RecurringJobDto>();

                        var dbNames = activeRecurring.Select(j => j.Name).ToHashSet(StringComparer.Ordinal);

                        var orphans = recurringJobs.Where(r => !dbNames.Contains(r.Id)).ToList();
                        _logger.LogInformation("[{Component}] [{TraceId}] found {Count} recurring jobs in Hangfire, {OrphanCount} orphans to remove", "JobSyncService", traceId, recurringJobs.Count(), orphans.Count);

                        foreach (var orphan in orphans)
                        {
                            if (cancellationToken.IsCancellationRequested) break;

                            try
                            {
                                await _scheduler.RemoveRecurringAsync(orphan.Id);
                                removed++;
                                _logger.LogInformation("[{Component}] [{TraceId}] removed orphan recurring job '{JobId}' from Hangfire", "JobSyncService", traceId, orphan.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "[{Component}] [{TraceId}] failed to remove orphan job '{JobId}'", "JobSyncService", traceId, orphan.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{Component}] [{TraceId}] error while scanning/removing orphan jobs", "JobSyncService", traceId);
                }

                _logger.LogInformation("[{Component}] [{TraceId}] synchronization completed. Registered: {Registered}, RemovedOrphans: {Removed}", "JobSyncService", traceId, registered, removed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Component}] [{TraceId}] unexpected error during synchronization", "JobSyncService", traceId);
            }
        }
    }

    public Task ExecuteJob(string jobType, string jobMethod)
    {
        // Execução dinâmica não implementada aqui; a sincronização registra jobs no Hangfire.
        _logger.LogInformation("[JobSyncService] ExecuteJob called for {JobType}.{JobMethod} - no direct execution implemented.", jobType, jobMethod);
        return Task.CompletedTask;
    }
}
