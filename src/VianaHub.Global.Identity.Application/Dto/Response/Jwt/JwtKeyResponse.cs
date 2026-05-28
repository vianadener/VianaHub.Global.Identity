namespace VianaHub.Global.Identity.Application.Dto.Response.Jwt;

public record JwtKeyResponse(
    int Id,
    int TenantId,
    Guid KeyId,
    string PublicKey,
    bool IsActive
);
