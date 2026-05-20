using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IJwtKeyDataRepository
{
    Task<JwtKeyEntity> GetByIdAsync(int id, CancellationToken ct);
    Task<JwtKeyEntity> GetByKeyIdAsync(Guid keyId, CancellationToken ct);
    Task<JwtKeyEntity> GetActiveKeyAsync(int tenantId, CancellationToken ct);
    Task<IEnumerable<JwtKeyEntity>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<JwtKeyEntity>> GetByTenantAsync(int tenantId, CancellationToken ct);
    Task<IEnumerable<JwtKeyEntity>> GetByApplicationAsync(int tenantId, CancellationToken ct);
    Task<IEnumerable<JwtKeyEntity>> GetKeysEligibleForRotationAsync(CancellationToken ct);
    Task<IEnumerable<JwtKeyEntity>> GetExpiredKeysAsync(int retentionDays, CancellationToken ct);
    Task<ListPage<JwtKeyEntity>> GetPagedAsync(PagedFilter request, int tenantId, CancellationToken ct);
    Task<bool> HasActiveKeyAsync(int tenantId, CancellationToken ct);
    Task<bool> CreateAsync(JwtKeyEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(JwtKeyEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(JwtKeyEntity entity, CancellationToken ct);
    Task<int> BulkUpdateTelemetryAsync(List<(int Id, long UsageCount, DateTime? LastUsedAt, long ValidationCount, DateTime? LastValidatedAt)> updates, CancellationToken ct);
}
