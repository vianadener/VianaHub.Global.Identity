using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace VianaHub.Global.Identity.Infra.Job.Services;

public class HangfireJobService : IJobSchedulerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HangfireJobService> _logger;
    private readonly IJobExecutor _jobExecutor;

    public HangfireJobService(IServiceProvider serviceProvider, ILogger<HangfireJobService> logger, IJobExecutor jobExecutor)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _jobExecutor = jobExecutor;
    }

    public async Task RegisterRecurringAsync(JobDefinitionEntity jobDef)
    {
        if (jobDef is null) throw new ArgumentNullException(nameof(jobDef));
        if (jobDef.ExecuteOnlyOnce) return;

        if (string.IsNullOrWhiteSpace(jobDef.CronExpression))
        {
            _logger.LogWarning("Skipping registration for recurring job '{JobName}' because CronExpression is empty", jobDef.Name);
            return;
        }

        // Resolve timezone safely, fallback to UTC if not found
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;
        if (!string.IsNullOrWhiteSpace(jobDef.TimeZoneId))
        {
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(jobDef.TimeZoneId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "TimeZone '{TimeZoneId}' not found for job '{JobName}', falling back to UTC", jobDef.TimeZoneId, jobDef.Name);
                timeZone = TimeZoneInfo.Utc;
            }
        }

        // Delegate to the concrete job executor which registers a stable invocation target for Hangfire
        var registered = await _jobExecutor.RegisterRecurringJobAsync(jobDef);
        if (registered)
        {
            _logger.LogInformation("Registered recurring job {JobName} in Hangfire", jobDef.Name);
        }
        else
        {
            _logger.LogWarning("Failed to register recurring job {JobName} via job executor", jobDef.Name);
        }
    }

    public async Task RemoveRecurringAsync(string jobName)
    {
        await _jobExecutor.RemoveRecurringJobAsync(jobName);
        _logger.LogInformation("Removed recurring job {JobName} from Hangfire", jobName);
    }

    public async Task<string> EnqueueJobAsync(JobDefinitionEntity jobDef)
    {
        if (jobDef is null) throw new ArgumentNullException(nameof(jobDef));

        var jobId = await _jobExecutor.EnqueueJobAsync(jobDef);
        _logger.LogInformation("Enqueued job {JobName} with Hangfire id {JobId}", jobDef.Name, jobId);
        return jobId;
    }
}
