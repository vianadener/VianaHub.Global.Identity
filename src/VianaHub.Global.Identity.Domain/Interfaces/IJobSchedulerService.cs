using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Domain.Interfaces;

/// <summary>
/// Abstração para agendamento e execução de jobs (adapter para Hangfire ou outros sistemas).
/// </summary>
public interface IJobSchedulerService
{
    Task RegisterRecurringAsync(JobDefinitionEntity jobDef);

    Task RemoveRecurringAsync(string jobName);

    Task<string> EnqueueJobAsync(JobDefinitionEntity jobDef);
}
