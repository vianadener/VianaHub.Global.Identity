namespace VianaHub.Global.Identity.Application.Dto.Response.Role;

public record RoleResponse(
    int Id,
    int AppId,
    string Name,
    bool IsActive
);