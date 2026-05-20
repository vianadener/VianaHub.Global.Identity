using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Interfaces;

public interface IJobDefinitionDataRepository
{
    Task<IEnumerable<JobDefinitionEntity>> GetAllAsync(CancellationToken ct);
    Task<JobDefinitionEntity> GetByIdAsync(int id, CancellationToken ct);
    Task<JobDefinitionEntity> GetByNameAsync(string jobName, CancellationToken ct);
    Task<ListPage<JobDefinitionEntity>> GetPagedAsync(PagedFilter filter, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string jobName, CancellationToken ct);
    Task<bool> CreateAsync(JobDefinitionEntity entity, CancellationToken ct);
    Task<bool> UpdateAsync(JobDefinitionEntity entity, CancellationToken ct);
    Task<bool> DeleteAsync(JobDefinitionEntity entity, CancellationToken ct);
}
