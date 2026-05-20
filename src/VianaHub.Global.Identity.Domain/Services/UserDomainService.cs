using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;

namespace VianaHub.Global.Identity.Domain.Services;

/// <summary>
/// Serviço de domínio para operações relacionadas a usuários
/// Centraliza validações e regras de negócio
/// </summary>
public class UserDomainService : IUserDomainService
{
    private readonly INotify _notify;
    private readonly IUserDataRepository _repo;
    private readonly IEntityDomainValidator<UserEntity> _validator;

    public UserDomainService(INotify notify,
                             IUserDataRepository repo,
                             IEntityDomainValidator<UserEntity> validator)
    {
        _notify = notify;
        _repo = repo;
        _validator = validator;
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync(int tenantId, CancellationToken ct)
    {
        return await _repo.GetAllAsync(tenantId, ct);
    }
    public async Task<UserEntity> GetByIdAsync(int tenantId, int id, CancellationToken ct)
    {
        return await _repo.GetByIdAsync(tenantId, id, ct);
    }
    public async Task<ListPage<UserEntity>> GetPagedAsync(int tenantId, PagedFilter request, CancellationToken ct)
    {
        return await _repo.GetPagedAsync(tenantId, request, ct);
    }
    public async Task<bool> ExistsByIdAsync(int tenantId, int id, CancellationToken ct)
    {
        return await _repo.ExistsByIdAsync(tenantId, id, ct);
    }

    public async Task<bool> CreateAsync(UserEntity entity, CancellationToken ct)
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
    public async Task<bool> UpdateAsync(UserEntity entity, CancellationToken ct)
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
    public async Task<bool> ActivateAsync(UserEntity entity, CancellationToken ct)
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
    public async Task<bool> DeactivateAsync(UserEntity entity, CancellationToken ct)
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
    public async Task<bool> DeleteAsync(UserEntity entity, CancellationToken ct)
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
