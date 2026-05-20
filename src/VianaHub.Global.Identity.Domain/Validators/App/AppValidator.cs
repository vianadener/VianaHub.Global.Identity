using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation.Results;

namespace VianaHub.Global.Identity.Domain.Validators.App;

public class AppValidator : BaseEntityValidator<AppEntity>
{
    public AppValidator(ILocalizationService localization) : base(localization)
    {
    }

    public override async Task<ValidationResult> ValidateForCreateAsync(AppEntity entity)
    {
        var validator = new CreateAppValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForUpdateAsync(AppEntity entity)
    {
        var validator = new UpdateAppValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForActivateAsync(AppEntity entity)
    {
        var validator = new ActivateAppValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForDeactivateAsync(AppEntity entity)
    {
        var validator = new DeactivateAppValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForDeleteAsync(AppEntity entity)
    {
        var validator = new DeleteAppValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override Task<ValidationResult> ValidateForRevokeAsync(AppEntity entity)
    {
        return Task.FromResult(new ValidationResult());
    }
}
