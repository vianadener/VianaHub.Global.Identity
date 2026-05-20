using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Resource;
using VianaHub.Global.Identity.Application.Dto.Response.Resource;
using Microsoft.AspNetCore.Http;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IResourceAppService
{
    Task<IEnumerable<ResourceResponse>> GetAllAsync(CancellationToken ct);
    Task<ResourceResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<ResourceResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct);
    
    Task<bool> CreateAsync(CreateResourceRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateResourceRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
