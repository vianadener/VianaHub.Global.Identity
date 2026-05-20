using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Role;
using VianaHub.Global.Identity.Application.Dto.Response.Role;
using Microsoft.AspNetCore.Http;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IRoleAppService
{
    Task<IEnumerable<RoleResponse>> GetAllAsync(CancellationToken ct);
    Task<RoleResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<RoleResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct);
    Task<bool> CreateAsync(CreateRoleRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateRoleRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
