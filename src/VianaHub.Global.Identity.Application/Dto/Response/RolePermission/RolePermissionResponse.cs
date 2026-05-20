namespace VianaHub.Global.Identity.Application.Dto.Response.RolePermission;

public class RolePermissionResponse
{
    public int Id { get; set; }
    public string Role { get; set; }
    public string Resource { get; set; }
    public string Action { get; set; }
}
