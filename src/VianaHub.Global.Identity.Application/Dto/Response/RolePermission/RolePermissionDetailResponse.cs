namespace VianaHub.Global.Identity.Application.Dto.Response.RolePermission;

public class RolePermissionDetailResponse
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string Role { get; set; }
    public int ResourceId { get; set; }
    public string Resource { get; set; }
    public int ActionId { get; set; }
    public string Action { get; set; }
}
