namespace VianaHub.Global.Identity.Application.Dto.Response.Auth;

public class AuthResponse
{
    public string TenantName { get; set; }
    public string UserName { get; set; }
    public string RoleName { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
}
