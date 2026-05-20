using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface ITenantDataRepository
{
    Task<TenantEntity> GetByIdAsync(int id, CancellationToken ct);
    Task<TenantEntity> GetByUserNameAsync(string email, CancellationToken ct);
    Task<TenantEntity> GetByLoginIdentifierAsync(string loginIdentifier, CancellationToken ct);
    Task<IEnumerable<TenantEntity>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<TenantEntity>> GetLoginAsync(CancellationToken ct);
    Task<ListPage<TenantEntity>> GetPagedAsync(PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
    Task<bool> CreateAsync(TenantEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(TenantEntity entity, CancellationToken ct);
}
