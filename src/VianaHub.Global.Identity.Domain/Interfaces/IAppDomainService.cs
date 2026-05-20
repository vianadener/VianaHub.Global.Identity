using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IAppDomainService
{
    Task<AppEntity> GetByIdAsync(int tenantId, int id, CancellationToken ct);
    Task<IEnumerable<AppEntity>> GetAllAsync(int tenantId, CancellationToken ct);
    Task<ListPage<AppEntity>> GetPagedAsync(int tenantId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int tenantId, int id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tenantId, string name, CancellationToken ct);
    Task<bool> CreateAsync(AppEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(AppEntity entity, CancellationToken ct);
    Task<bool> ActivateAsync(AppEntity entity, CancellationToken ct);
    Task<bool> DeactivateAsync(AppEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(AppEntity entity, CancellationToken ct);
}
