namespace VianaHub.Global.Identity.Application.Dto.Response.Jwt;

public class JwtKeyDetailResponse
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Tenant { get; set; }
    public Guid KeyId { get; set; }
    public string PublicKey { get; set; } = string.Empty;
    public string Algorithm { get; set; } = "RS256";
    public int KeySize { get; set; }
    public string KeyType { get; set; } = "RSA";
    public string RevokedReason { get; set; } = string.Empty;
    public long UsageCount { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime NextRotationAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? LastValidatedAt { get; set; }
    public long ValidationCount { get; set; }
    public int RotationPolicyDays { get; set; }
    public int OverlapPeriodDays { get; set; }
    public int MaxTokenLifetimeMinutes { get; set; }
    public bool IsActive { get; set; }
}
