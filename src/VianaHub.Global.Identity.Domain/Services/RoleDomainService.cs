using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Middleware.Lib.Notifications;

namespace VianaHub.Global.Identity.Domain.Services;

public class RoleDomainService : IRoleDomainService
{
    private readonly IRoleDataRepository _repo;
    private readonly IEntityDomainValidator<RoleEntity> _validator;
    private readonly INotify _notify;

    public RoleDomainService(
        IRoleDataRepository repo,
        IEntityDomainValidator<RoleEntity> validator,
        INotify notify)
    {
        _repo = repo;
        _validator = validator;
        _notify = notify;
    }

    public async Task<IEnumerable<RoleEntity>> GetAllAsync(int tanantId, int appId, CancellationToken ct)
    {
        return await _repo.GetAllAsync(tanantId, appId, ct);
    }
    public async Task<RoleEntity> GetByIdAsync(int tanantId, int appId, int id, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(tanantId, appId, id, ct);
    }
    public async Task<ListPage<RoleEntity>> GetPagedAsync(int tanantId, int appId, PagedFilter request, CancellationToken ct)
    {
        return await _repo.GetPagedAsync(tanantId, appId, request, ct);
    }
    public async Task<bool> ExistsByNameAsync(int tenantId, int appId, string name, CancellationToken ct)
    {
        return await _repo.ExistsByNameAsync(tenantId, appId, name, ct);
    }

    public async Task<bool> CreateAsync(RoleEntity entity, CancellationToken ct)
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
    public async Task<bool> UpdateAsync(RoleEntity entity, CancellationToken ct)
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
    public async Task<bool> ActivateAsync(RoleEntity entity, CancellationToken ct)
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
    public async Task<bool> DeactivateAsync(RoleEntity entity, CancellationToken ct)
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
    public async Task<bool> DeleteAsync(RoleEntity entity, CancellationToken ct)
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
