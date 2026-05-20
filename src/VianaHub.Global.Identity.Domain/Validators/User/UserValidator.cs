using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;
using FluentValidation.Results;

namespace VianaHub.Global.Identity.Domain.Validators.User;

/// <summary>
/// Validador base para UserEntity com regras comuns
/// </summary>
public class UserValidator : AbstractValidator<UserEntity>, IEntityDomainValidator<UserEntity>
{
    protected readonly ILocalizationService _localization;

    public UserValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .When(x => x.Id != 0)
            .WithMessage(_localization.GetMessage("Domain.User.InvalidId"));
    }

    protected void ValidateTenantId()
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage(_localization.GetMessage("Domain.User.TenantIdRequired"));
    }

    protected void ValidateName()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_localization.GetMessage("Domain.User.NameRequired"))
            .MaximumLength(150)
            .WithMessage(_localization.GetMessage("Domain.User.NameMaxLength", 150));
    }

    protected void ValidatePasswordHash()
    {
        RuleFor(x => x.PasswordHash)
            .NotEmpty()
            .WithMessage(_localization.GetMessage("Domain.User.PasswordHashRequired"))
            .MinimumLength(50)
            .WithMessage(_localization.GetMessage("Domain.User.PasswordHashInvalid"))
            .MaximumLength(500)
            .WithMessage(_localization.GetMessage("Domain.User.PasswordHashInvalid"));
    }

    protected void ValidateNotDeleted()
    {
        RuleFor(x => x.IsDeleted)
            .Equal(false)
            .WithMessage(_localization.GetMessage("Domain.User.CannotModifyDeleted"));
    }

    public async Task<ValidationResult> ValidateForCreateAsync(UserEntity entity)
    {
        var validator = new CreateUserValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public async Task<ValidationResult> ValidateForUpdateAsync(UserEntity entity)
    {
        var validator = new UpdateUserValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public async Task<ValidationResult> ValidateForActivateAsync(UserEntity entity)
    {
        var validator = new ActivateUserValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public async Task<ValidationResult> ValidateForDeactivateAsync(UserEntity entity)
    {
        var validator = new DeactivateUserValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public async Task<ValidationResult> ValidateForDeleteAsync(UserEntity entity)
    {
        var validator = new DeleteUserValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public async Task<ValidationResult> ValidateForRevokeAsync(UserEntity entity)
    {
        // User não tem operação de revoke, então retorna sucesso
        return await Task.FromResult(new ValidationResult());
    }
}
