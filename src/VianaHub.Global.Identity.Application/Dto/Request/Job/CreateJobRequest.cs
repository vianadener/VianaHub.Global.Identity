namespace VianaHub.Global.Identity.Application.Dto.Request.Job;

public record CreateJobRequest(
    string JobCategory,
    string JobName,
    string Description,
    string JobPurpose,
    string JobType,
    string JobConfiguration,
    string CronExpression,
    string JobMethod = "Execute",
    string TimeZoneId = "GMT Standard Time",
    bool ExecuteOnlyOnce = false,
    int TimeoutMinutes = 5,
    int Priority = 5,
    string Queue = "default",
    int MaxRetries = 3,
    bool IsSystemJob = false
);
