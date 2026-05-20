using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IRoleDomainService
{
    Task<IEnumerable<RoleEntity>> GetAllAsync(int tanantId, int appId, CancellationToken ct);
    Task<RoleEntity> GetByIdAsync(int tanantId, int appId, int id, CancellationToken ct);
    Task<ListPage<RoleEntity>> GetPagedAsync(int tanantId, int appId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tenantId, int appId, string name, CancellationToken ct);

    Task<bool> CreateAsync(RoleEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(RoleEntity entity, CancellationToken ct);
    Task<bool> ActivateAsync(RoleEntity entity, CancellationToken ct);
    Task<bool> DeactivateAsync(RoleEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(RoleEntity entity, CancellationToken ct);
}
