using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IPasswordResetTokenDataRepository
{
    Task<PasswordResetTokenEntity?> GetByTokenHashAsync(byte[] tokenHash, CancellationToken ct);
    Task<int> CountRecentByUserAsync(int userId, int tenantId, DateTime since, CancellationToken ct);
    Task<bool> CreateAsync(PasswordResetTokenEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(PasswordResetTokenEntity entity, CancellationToken ct);
}
