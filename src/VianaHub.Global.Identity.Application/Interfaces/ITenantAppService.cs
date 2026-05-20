using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Tenant;
using VianaHub.Global.Identity.Application.Dto.Response.Tenant;
using Microsoft.AspNetCore.Http;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface ITenantAppService
{
    Task<IEnumerable<TenantResponse>> GetAllAsync(CancellationToken ct);
    Task<TenantDetailResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<TenantResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct);
    Task<bool> CreateAsync(CreateTenantRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateTenantRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
