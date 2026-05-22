using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Middleware.Lib.Notifications;

namespace VianaHub.Global.Identity.Domain.Services;

public class AppDomainService : IAppDomainService
{
    private readonly IAppDataRepository _repo;
    private readonly IEntityDomainValidator<AppEntity> _validator;
    private readonly INotify _notify;

    public AppDomainService(
        IAppDataRepository repo,
        IEntityDomainValidator<AppEntity> validator,
        INotify notify)
    {
        _repo = repo;
        _validator = validator;
        _notify = notify;
    }

    public async Task<AppEntity> GetByIdAsync(int tenantId, int id, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(tenantId, id, ct);
    }

    public async Task<IEnumerable<AppEntity>> GetAllAsync(int tenantId, CancellationToken ct)
    {
        return await _repo.GetAllAsync(tenantId, ct);
    }

    public async Task<ListPage<AppEntity>> GetPagedAsync(int tenantId, PagedFilter request, CancellationToken ct)
    {
        return await _repo.GetPagedAsync(tenantId, request, ct);
    }

    public async Task<bool> ExistsByIdAsync(int tenantId, int id, CancellationToken ct)
    {
        return await _repo.ExistsByIdAsync(tenantId, id, ct);
    }

    public async Task<bool> ExistsByNameAsync(int tenantId, string name, CancellationToken ct)
    {
        return await _repo.ExistsByNameAsync(tenantId, name, ct);
    }

    public async Task<bool> CreateAsync(AppEntity entity, CancellationToken ct)
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

    public async Task<bool> UpdateAsync(AppEntity entity, CancellationToken ct)
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

    public async Task<bool> ActivateAsync(AppEntity entity, CancellationToken ct)
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

    public async Task<bool> DeactivateAsync(AppEntity entity, CancellationToken ct)
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

    public async Task<bool> DeleteAsync(AppEntity entity, CancellationToken ct)
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
