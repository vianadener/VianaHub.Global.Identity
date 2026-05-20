using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IUserDataRepository
{
    Task<IEnumerable<UserEntity>> GetAllAsync(int tenatId, CancellationToken ct);
    Task<UserEntity> GetByIdAsync(int tenatId, int id, CancellationToken ct);
    Task<UserEntity> GetByNormalizedLoginAsync(int tenatId, string email, CancellationToken ct);
    Task<ListPage<UserEntity>> GetPagedAsync(int tenatId, PagedFilter filter, CancellationToken ct);
    Task<bool> ExistsByIdAsync(int tenatId, int id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tenatId, string email, CancellationToken ct);
    Task<bool> CreateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(UserEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(UserEntity entity, CancellationToken ct);
}
