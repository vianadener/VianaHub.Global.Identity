using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;


namespace VianaHub.Global.Identity.Domain.Services;

public class UserRoleDomainService : IUserRoleDomainService
{
    private readonly INotify _notify;
    private readonly IUserRoleDataRepository _repository;
    private readonly IEntityDomainValidator<UserRoleEntity> _validator;

    public UserRoleDomainService(INotify notify, 
                                 IUserRoleDataRepository repository, 
                                 IEntityDomainValidator<UserRoleEntity> validator)
    {
        _notify = notify;
        _repository = repository;
        _validator = validator;
    }

    public async Task<IList<UserRoleEntity>> GetAllAsync(int tenantId, int appId, CancellationToken ct)
    {
        return await _repository.GetAllAsync(tenantId, appId, ct);
    }
    public async Task<UserRoleEntity> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _repository.GetByIdAsync(id, ct);
    }
    public async Task<ListPage<UserRoleEntity>> GetPagedAsync(int tenantId, int appId, PagedFilter request, CancellationToken ct)
    {
        return await _repository.GetPagedAsync(tenantId, appId, request, ct);
    }
    public async Task<bool> ExistsAsync(int tenantId, int appId, int userId, int roleId, CancellationToken ct)
    {
        return await _repository.ExistsAsync(tenantId, appId, userId, roleId, ct);
    }

    public async Task<bool> CreateAsync(UserRoleEntity entity, CancellationToken ct)
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
        return await _repository.CreateAsync(entity, ct);
    }
    public async Task<bool> DeleteAsync(UserRoleEntity entity, CancellationToken ct)
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

        return await _repository.DeleteAsync(entity, ct);
    }
}
