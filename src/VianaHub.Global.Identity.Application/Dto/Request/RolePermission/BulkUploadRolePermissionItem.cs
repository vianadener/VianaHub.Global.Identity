namespace VianaHub.Global.Identity.Application.Dto.Request.RolePermission;

public class BulkUploadRolePermissionItem
{
    public int AppId { get; set; }
    public int RoleId { get; set; }
    public int ResourceId { get; set; }
    public int ActionId { get; set; }
}
