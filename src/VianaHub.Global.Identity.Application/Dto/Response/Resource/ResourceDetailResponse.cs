namespace VianaHub.Global.Identity.Application.Dto.Response.Resource;

public record ResourceDetailResponse(
    int Id,
    int AppId,
    string Name,
    string Description,
    bool IsActive
);
