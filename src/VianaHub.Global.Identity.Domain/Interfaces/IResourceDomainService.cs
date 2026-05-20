using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IResourceDomainService
{
    Task<IEnumerable<ResourceEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct);
    Task<ResourceEntity> GetByIdAsync(int tenantId, int appId, int id, CancellationToken ct);
    Task<ListPage<ResourceEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tenantId, int appId, string name, CancellationToken ct);

    Task<bool> CreateAsync(ResourceEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(ResourceEntity entity, CancellationToken ct);
    Task<bool> ActivateAsync(ResourceEntity entity, CancellationToken ct);
    Task<bool> DeactivateAsync(ResourceEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(ResourceEntity entity, CancellationToken ct);
}
