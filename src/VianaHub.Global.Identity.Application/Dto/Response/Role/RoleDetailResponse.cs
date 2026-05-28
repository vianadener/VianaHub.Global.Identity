namespace VianaHub.Global.Identity.Application.Dto.Response.Role;

public record RoleDetailResponse(
    int Id,
    int AppId,
    string Name,
    string Description,
    bool IsActive
);