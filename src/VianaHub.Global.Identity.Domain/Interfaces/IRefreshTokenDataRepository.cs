using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IRefreshTokenDataRepository
{
    Task<RefreshTokenEntity> GetByTokenHashAsync(byte[] tokenHash, int tenantId, CancellationToken ct);
    Task<IEnumerable<RefreshTokenEntity>> GetByUserAsync(int userId, int tenantId, CancellationToken ct);
    Task<bool> CreateAsync(RefreshTokenEntity entity, CancellationToken ct);
    Task<bool> RevokeAsync(RefreshTokenEntity entity, CancellationToken ct);
    Task<int> RevokeAllByUserAsync(int userId, int tenantId, int revokedBy, CancellationToken ct);
}
