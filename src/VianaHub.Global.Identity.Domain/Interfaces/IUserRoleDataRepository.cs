using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IUserRoleDataRepository
{
    Task<IList<UserRoleEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct);
    Task<UserRoleEntity> GetByIdAsync(int id, CancellationToken ct);
    Task<IList<UserRoleEntity>> GetByUserIdAsync(int userId, CancellationToken ct);
    Task<ListPage<UserRoleEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsAsync(int tenantId, int appId, int userId, int roleId, CancellationToken ct);

    Task<bool> CreateAsync(UserRoleEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(UserRoleEntity entity, CancellationToken ct);
}
