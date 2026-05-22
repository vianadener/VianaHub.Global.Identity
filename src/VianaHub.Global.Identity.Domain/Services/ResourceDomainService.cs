using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Middleware.Lib.Notifications;

namespace VianaHub.Global.Identity.Domain.Services;

public class ResourceDomainService : IResourceDomainService
{
    private readonly IResourceDataRepository _repo;
    private readonly IEntityDomainValidator<ResourceEntity> _validator;
    private readonly INotify _notify;

    public ResourceDomainService(
        IResourceDataRepository repo,
        IEntityDomainValidator<ResourceEntity> validator,
        INotify notify)
    {
        _repo = repo;
        _validator = validator;
        _notify = notify;
    }

    public async Task<IEnumerable<ResourceEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct)
    {
        return await _repo.GetAllAsync(tenantId, appId, ct);
    }
    public async Task<ResourceEntity> GetByIdAsync(int tenantId, int appId, int id, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(tenantId, appId, id, ct);
    }
    public async Task<ListPage<ResourceEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct)
    {
        return await _repo.GetPagedAsync(tenantId, appId, request, ct);
    }
    public async Task<bool> ExistsByNameAsync(int tenantId, int appId, string name, CancellationToken ct)
    {
        return await _repo.ExistsByNameAsync(tenantId, appId, name, ct);
    }

    public async Task<bool> CreateAsync(ResourceEntity entity, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateForCreateAsync(entity);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                _notify.Add(error.ErrorMessage, 400);
            }
            return false;
        }

        return await _repo.CreateAsync(entity, ct);
    }
    public async Task<bool> UpdateAsync(ResourceEntity entity, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateForUpdateAsync(entity);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                _notify.Add(error.ErrorMessage, 400);
            }
            return false;
        }

        return await _repo.UpdateAsync(entity, ct);
    }
    public async Task<bool> ActivateAsync(ResourceEntity entity, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateForActivateAsync(entity);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                _notify.Add(error.ErrorMessage, 400);
            }
            return false;
        }

        return await _repo.UpdateAsync(entity, ct);
    }
    public async Task<bool> DeactivateAsync(ResourceEntity entity, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateForDeactivateAsync(entity);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                _notify.Add(error.ErrorMessage, 400);
            }
            return false;
        }

        return await _repo.UpdateAsync(entity, ct);
    }
    public async Task<bool> DeleteAsync(ResourceEntity entity, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateForDeleteAsync(entity);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                _notify.Add(error.ErrorMessage, 400);
            }
            return false;
        }

        return await _repo.UpdateAsync(entity, ct);
    }
}
