using VianaHub.Global.Identity.Application.Dto.Request.RolePermission;
using VianaHub.Global.Identity.Application.Dto.Response.RolePermission;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.AspNetCore.Http;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IRolePermissionAppService
{
    Task<IList<RolePermissionResponse>> GetAllAsync(CancellationToken ct);
    Task<RolePermissionDetailResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPage<RolePermissionResponse>> GetPagedAsync(PagedFilter request, CancellationToken ct);

    Task<RolePermissionResponse> CreateAsync(CreateRolePermissionRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
