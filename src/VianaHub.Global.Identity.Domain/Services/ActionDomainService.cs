using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Middleware.Lib.Notifications;

namespace VianaHub.Global.Identity.Domain.Services;

public class ActionDomainService : IActionDomainService
{
    private readonly IActionDataRepository _repo;
    private readonly IEntityDomainValidator<ActionEntity> _validator;
    private readonly INotify _notify;

    public ActionDomainService(
        IActionDataRepository repo,
        IEntityDomainValidator<ActionEntity> validator,
        INotify notify)
    {
        _repo = repo;
        _validator = validator;
        _notify = notify;
    }

    public async Task<IEnumerable<ActionEntity>> GetAllAsync(int tanantId, int appId, CancellationToken ct)
    {
        return await _repo.GetAllAsync(tanantId, appId, ct);
    }
    public async Task<ActionEntity> GetByIdAsync(int tanantId, int appId, int id, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(tanantId, appId, id, ct);
    }
    public async Task<ListPage<ActionEntity>> GetPagedAsync(int tanantId, int appId, PagedFilter request, CancellationToken ct)
    {
        return await _repo.GetPagedAsync(tanantId, appId, request, ct);
    }
    public async Task<bool> ExistsByNameAsync(int tanantId, int appId, string name, CancellationToken ct)
    {
        return await _repo.ExistsByNameAsync(tanantId, appId, name, ct);
    }

    public async Task<bool> CreateAsync(ActionEntity entity, CancellationToken ct)
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
    public async Task<bool> UpdateAsync(ActionEntity entity, CancellationToken ct)
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
    public async Task<bool> ActivateAsync(ActionEntity entity, CancellationToken ct)
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
    public async Task<bool> DeactivateAsync(ActionEntity entity, CancellationToken ct)
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
    public async Task<bool> DeleteAsync(ActionEntity entity, CancellationToken ct)
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
