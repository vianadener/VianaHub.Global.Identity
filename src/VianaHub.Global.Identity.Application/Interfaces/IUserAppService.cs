using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.User;
using VianaHub.Global.Identity.Application.Dto.Response.User;
using Microsoft.AspNetCore.Http;

namespace VianaHub.Global.Identity.Application.Interfaces;

public interface IUserAppService
{
    Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken ct);
    Task<UserResponse> GetByIdAsync(int id, CancellationToken ct);
    Task<ListPageResponse<UserResponse>> GetPagedAsync(PagedFilterRequest request, CancellationToken ct);
    Task<bool> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct);
    Task<bool> UpdatePasswordAsync(int id, UpdateSecretRequest request, CancellationToken ct);
    Task<bool> ActivateAsync(int id, CancellationToken ct);
    Task<bool> DeactivateAsync(int id, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<bool> BulkUploadAsync(IFormFile file, CancellationToken ct);
}
