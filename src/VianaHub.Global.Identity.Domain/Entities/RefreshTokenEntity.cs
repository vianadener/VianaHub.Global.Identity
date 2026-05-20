using VianaHub.Global.Identity.Domain.Base;

namespace VianaHub.Global.Identity.Domain.Entities;

/// <summary>
/// Refresh token persistido para permitir rotação e revogação
/// </summary>
public class RefreshTokenEntity : Entity
{
    public int TenantId { get; private set; }
    public int AppId { get; private set; }
    public int UserId { get; private set; }
    public byte[]? TokenHash { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public int? RevokedBy { get; private set; }

    // Navigation Properties
    public UserEntity? User { get; private set; }

    protected RefreshTokenEntity() { }

    public RefreshTokenEntity(int tenantId, int appId, int userId, byte[] tokenHash, DateTime? expiresAt, int createdBy)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido", nameof(tenantId));
        if (appId <= 0) throw new ArgumentException("AppId inválido", nameof(appId));
        if (userId <= 0) throw new ArgumentException("UserId inválido", nameof(userId));
        if (tokenHash == null || tokenHash.Length == 0) throw new ArgumentException("TokenHash inválido", nameof(tokenHash));

        TenantId = tenantId;
        AppId = appId;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        AddedOn = DateTime.UtcNow;
        AddedBy = createdBy;
    }

    public bool IsActive() => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

    public void Revoke(int revokedBy)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        ModifiedBy = revokedBy;
        ModifiedAt = DateTime.UtcNow;
    }
}
