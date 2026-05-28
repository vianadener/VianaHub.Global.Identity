namespace VianaHub.Global.Identity.Application.Dto.Response.RolePermission;

public record RolePermissionDetailResponse(
    int Id,
    int RoleId,
    string Role,
    int ResourceId,
    string Resource,
    int ActionId,
    string Action
);