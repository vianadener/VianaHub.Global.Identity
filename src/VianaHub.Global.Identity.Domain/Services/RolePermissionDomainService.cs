using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Services;

public class RolePermissionDomainService : IRolePermissionDomainService
{
    private readonly IRolePermissionDataRepository _repository;
    private readonly IValidator<RolePermissionEntity> _validator;

    public RolePermissionDomainService(IRolePermissionDataRepository repository, IValidator<RolePermissionEntity> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<IList<RolePermissionEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct)
    {
        return await _repository.GetAllAsync(tenantId, appId, ct);
    }
    public async Task<RolePermissionEntity> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _repository.GetByIdAsync(id, ct);
    }
    public async Task<ListPage<RolePermissionEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct)
    {
        return await _repository.GetPagedAsync(tenantId, appId, request, ct);
    }
    public async Task<bool> ExistsAsync(int tenantId, int appId, int roleId, int resourceId, int actionId, CancellationToken ct)
    {
        return await _repository.ExistsAsync(tenantId, appId, roleId, resourceId, actionId, ct);
    }

    public async Task<bool> CreateAsync(RolePermissionEntity entity, CancellationToken ct)
    {
        await _repository.CreateAsync(entity, ct);
        return true;
    }
    public async Task<bool> DeleteAsync(RolePermissionEntity entity, CancellationToken ct)
    {
        return await _repository.DeleteAsync(entity, ct);
    }
}
