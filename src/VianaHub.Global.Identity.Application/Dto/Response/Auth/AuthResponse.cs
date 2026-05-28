namespace VianaHub.Global.Identity.Application.Dto.Response.Auth;

public record AuthResponse(
    string? TenantName,
    string? UserName,
    string? RoleName,
    DateTime AccessTokenExpiresAt
);