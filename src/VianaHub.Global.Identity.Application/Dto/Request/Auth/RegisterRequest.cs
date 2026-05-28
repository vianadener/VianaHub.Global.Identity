namespace VianaHub.Global.Identity.Application.Dto.Request.Auth;

public record RegisterRequest(int TenantId, string Name, string Secret, string UrlImage);