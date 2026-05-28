namespace VianaHub.Global.Identity.Application.Dto.Response.Job;

public record JobResponse(
    int Id,
    string JobCategory,
    string JobName,
    string CronExpression,
    int Priority,
    bool IsActive
);
