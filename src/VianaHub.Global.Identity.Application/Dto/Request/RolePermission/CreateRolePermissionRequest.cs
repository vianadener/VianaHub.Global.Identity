namespace VianaHub.Global.Identity.Application.Dto.Request.RolePermission;

public class CreateRolePermissionRequest
{
    public int RoleId { get; set; }
    public int ResourceId { get; set; }
    public int ActionId { get; set; }
}
