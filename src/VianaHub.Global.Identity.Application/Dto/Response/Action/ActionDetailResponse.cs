namespace VianaHub.Global.Identity.Application.Dto.Response.Action;

public record ActionDetailResponse(
    int Id,
    string Name,
    string Description,
    bool IsActive
);