using VianaHub.Global.Identity.Application.Dto.Request.UserRole;
using VianaHub.Global.Identity.Application.Dto.Response.UserRole;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.AspNetCore.Http;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IUserRoleAppService
{
    Task<IList<UserRoleResponse>> GetAllAsync(CancellationToken ct);
    Task<UserRoleResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPage<UserRoleEntity>> GetPagedAsync(PagedFilter request, CancellationToken ct);

    Task<UserRoleResponse> CreateAsync(CreateUserRoleRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
