namespace VianaHub.Global.Identity.Application.Dto.Request.Auth;

public record RefreshRequest(int TenantId, string RefreshToken);