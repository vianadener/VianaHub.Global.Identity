using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IRolePermissionDataRepository
{
    Task<IList<RolePermissionEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct);
    Task<RolePermissionEntity> GetByIdAsync(int id, CancellationToken ct);
    Task<RolePermissionEntity> GetByRoleIdAsync(int roleId, CancellationToken ct);
    Task<IList<RolePermissionEntity>> GetByRoleAsync(int roleId, int tenantId, CancellationToken ct);
    Task<ListPage<RolePermissionEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsAsync(int tenantId, int appId, int roleId, int resourceId, int actionId, CancellationToken ct);

    Task<bool> CreateAsync(RolePermissionEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(RolePermissionEntity entity, CancellationToken ct);
}
