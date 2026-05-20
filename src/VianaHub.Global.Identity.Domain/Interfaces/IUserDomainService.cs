using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IUserDomainService
{
    Task<IEnumerable<UserEntity>> GetAllAsync(int tenantId, CancellationToken ct);
    Task<UserEntity> GetByIdAsync(int tenantId, int id, CancellationToken ct);
    Task<ListPage<UserEntity>> GetPagedAsync(int tenantId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int tenantId, int id, CancellationToken ct);

    Task<bool> CreateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> ActivateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> DeactivateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(UserEntity entity, CancellationToken ct);
}
