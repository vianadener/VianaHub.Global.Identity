namespace VianaHub.Global.Identity.Application.Dto.Response.Action;

public record ActionResponse(
    int Id,
    string Name,
    bool IsActive
);