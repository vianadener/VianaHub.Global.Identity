namespace VianaHub.Global.Identity.Application.Dto.Request.RolePermission;

public record CreateRolePermissionRequest(int RoleId, int ResourceId, int ActionId);