using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IActionDataRepository
{
    Task<IEnumerable<ActionEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct);
    Task<ActionEntity> GetByIdAsync(int tenantId, int appId, int id, CancellationToken ct);
    Task<ListPage<ActionEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct);
    Task<bool> ExistsByNameAsync(int tenantId, int appId, string name, CancellationToken ct);

    Task<bool> CreateAsync(ActionEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(ActionEntity entity, CancellationToken ct);
}
