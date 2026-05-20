namespace VianaHub.Global.Identity.Application.Dto.Request.UserRole;

public class CreateUserRoleRequest
{
    public int AppId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
