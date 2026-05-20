using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IRoleDataRepository
{
    Task<IEnumerable<RoleEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct);
    Task<RoleEntity> GetByIdAsync(int tenantId, int appId, int id, CancellationToken ct);
    Task<ListPage<RoleEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tenantId, int appId, string name, CancellationToken ct);

    Task<bool> CreateAsync(RoleEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(RoleEntity entity, CancellationToken ct);
}
