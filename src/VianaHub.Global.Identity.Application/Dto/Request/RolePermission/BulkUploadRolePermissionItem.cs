namespace VianaHub.Global.Identity.Application.Dto.Request.RolePermission;

public record BulkUploadRolePermissionItem(int AppId, int RoleId, int ResourceId, int ActionId);