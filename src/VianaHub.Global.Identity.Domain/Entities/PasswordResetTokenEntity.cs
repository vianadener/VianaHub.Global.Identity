using VianaHub.Global.Identity.Domain.Base;

namespace VianaHub.Global.Identity.Domain.Entities;

/// <summary>
/// Token de reset de senha — one-time use, TTL de 15 minutos, hash SHA-256 armazenado
/// </summary>
public class PasswordResetTokenEntity : Entity
{
    public int TenantId { get; private set; }
    public int UserId { get; private set; }
    public byte[] TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool Used { get; private set; }

    // Navigation Properties
    public UserEntity? User { get; private set; }

    protected PasswordResetTokenEntity() { }

    public PasswordResetTokenEntity(int tenantId, int userId, byte[] tokenHash, DateTime expiresAt, int createdBy)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido", nameof(tenantId));
        if (userId <= 0) throw new ArgumentException("UserId inválido", nameof(userId));
        if (tokenHash is null || tokenHash.Length == 0) throw new ArgumentException("TokenHash inválido", nameof(tokenHash));
        if (expiresAt <= DateTime.UtcNow) throw new ArgumentException("ExpiresAt deve ser uma data futura", nameof(expiresAt));

        TenantId = tenantId;
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        Used = false;
        AddedBy = createdBy;
        AddedOn = DateTime.UtcNow;
    }

    public bool IsValid() => !Used && DateTime.UtcNow < ExpiresAt;

    public void MarkAsUsed(int modifiedBy)
    {
        Used = true;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }
}
