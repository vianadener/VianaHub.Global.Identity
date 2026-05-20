namespace VianaHub.Global.Identity.Application.Dto.Response.Auth;

public class AuthDetailResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public int TenantId { get; set; }
    public string? TenantName { get; set; }
    public int AppId { get; set; }
    public string? AppName { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
}
