using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;
using FluentValidation.Results;

namespace VianaHub.Global.Identity.Domain.Validators.UserRole;

/// <summary>
/// Validador base para UserRoleEntity com regras comuns
/// </summary>
public class UserRoleValidator : AbstractValidator<UserRoleEntity>, IEntityDomainValidator<UserRoleEntity>
{
    protected readonly ILocalizationService _localization;

    public UserRoleValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .When(x => x.Id != 0)
            .WithMessage(_localization.GetMessage("Domain.UserRole.InvalidId"));
    }

    protected void ValidateTenantId()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage(_localization.GetMessage("Domain.UserRole.TenantIdRequired"));
    }

    protected void ValidateAppId()
    {
        RuleFor(x => x.AppId)
            .GreaterThan(0)
            .WithMessage(_localization.GetMessage("Domain.UserRole.AppIdRequired"));
    }

    protected void ValidateUserId()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage(_localization.GetMessage("Domain.UserRole.UserIdRequired"));
    }

    protected void ValidateRoleId()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage(_localization.GetMessage("Domain.UserRole.RoleIdRequired"));
    }

    public async Task<ValidationResult> ValidateForCreateAsync(UserRoleEntity entity)
    {
        var validator = new CreateUserRoleValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public async Task<ValidationResult> ValidateForDeleteAsync(UserRoleEntity entity)
    {
        var validator = new DeleteUserRoleValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public Task<ValidationResult> ValidateForUpdateAsync(UserRoleEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateForActivateAsync(UserRoleEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateForDeactivateAsync(UserRoleEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateForRevokeAsync(UserRoleEntity entity)
    {
        throw new NotImplementedException();
    }
}
