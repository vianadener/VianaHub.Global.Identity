namespace VianaHub.Global.Identity.Application.Dto.Response.UserRole;

public class UserRoleDetailResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Tenant { get; set; }
    public int UserId { get; set; }
    public string User { get; set; }
    public int RoleId { get; set; }
    public string Role { get; set; }
    public string UserName { get; set; }
    public string RoleName { get; set; }
}
