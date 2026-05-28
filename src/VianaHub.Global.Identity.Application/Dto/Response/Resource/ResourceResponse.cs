namespace VianaHub.Global.Identity.Application.Dto.Response.Resource;

public record ResourceResponse(
    int Id,
    int AppId,
    string Name,
    bool IsActive
);
