namespace VianaHub.Global.Identity.Application.Dto.Request.Job;

public record UpdateJobRequest(

    string Description,
    string JobPurpose,
    string CronExpression,
    string TimeZoneId,
    int TimeoutMinutes,
    int Priority,
    string Queue,
    int MaxRetries,
    string JobConfiguration,
    bool IsActive
);