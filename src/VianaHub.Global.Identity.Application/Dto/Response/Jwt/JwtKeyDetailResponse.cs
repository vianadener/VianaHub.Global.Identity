namespace VianaHub.Global.Identity.Application.Dto.Response.Jwt;

public record JwtKeyDetailResponse(
    int Id,
    int TenantId,
    string Tenant,
    Guid KeyId,
    string PublicKey,
    string Algorithm,
    int KeySize,
    string KeyType,
    string RevokedReason,
    long UsageCount,
    DateTime? ActivatedAt,
    DateTime ExpiresAt,
    DateTime? LastUsedAt,
    DateTime NextRotationAt,
    DateTime? RevokedAt,
    DateTime? LastValidatedAt,
    long ValidationCount,
    int RotationPolicyDays,
    int OverlapPeriodDays,
    int MaxTokenLifetimeMinutes,
    bool IsActive
);