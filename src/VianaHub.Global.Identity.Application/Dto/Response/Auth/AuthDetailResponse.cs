namespace VianaHub.Global.Identity.Application.Dto.Response.Auth;

public record AuthDetailResponse(
    string? AccessToken,
    string? RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime? RefreshTokenExpiresAt,
    int TenantId,
    string? TenantName,
    int AppId,
    string? AppName,
    int UserId,
    string? UserName,
    int RoleId,
    string? RoleName
);
