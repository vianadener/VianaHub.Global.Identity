using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Job;
using VianaHub.Global.Identity.Application.Dto.Response.Job;
using VianaHub.Global.Identity.Domain.ReadModels;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IJobAppService
{
    Task<IEnumerable<JobResponse>> GetAllAsync(CancellationToken ct);
    Task<JobDetailResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<JobResponse>> GetPagedAsync(JobPagedFilter request, CancellationToken ct);
    Task<bool> CreateAsync(CreateJobRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateJobRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> ExecuteAsync(int id, CancellationToken ct);
}
