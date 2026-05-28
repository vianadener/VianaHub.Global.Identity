namespace VianaHub.Global.Identity.Application.Dto.Response.RolePermission;

public record RolePermissionResponse(
    int Id,
    string Role,
    string Resource,
    string Action
);