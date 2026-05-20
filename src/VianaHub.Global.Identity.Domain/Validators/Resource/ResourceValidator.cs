using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation.Results;

namespace VianaHub.Global.Identity.Domain.Validators.Resource;

/// <summary>
/// Validador completo para ResourceEntity
/// </summary>
public class ResourceValidator : BaseEntityValidator<ResourceEntity>
{
    public ResourceValidator(ILocalizationService localization) : base(localization)
    {
    }

    public override async Task<ValidationResult> ValidateForCreateAsync(ResourceEntity entity)
    {
        var validator = new CreateResourceValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForUpdateAsync(ResourceEntity entity)
    {
        var validator = new UpdateResourceValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForActivateAsync(ResourceEntity entity)
    {
        var validator = new ActivateResourceValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForDeactivateAsync(ResourceEntity entity)
    {
        var validator = new DeactivateResourceValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override async Task<ValidationResult> ValidateForDeleteAsync(ResourceEntity entity)
    {
        var validator = new DeleteResourceValidator(_localization);
        return await validator.ValidateAsync(entity);
    }

    public override Task<ValidationResult> ValidateForRevokeAsync(ResourceEntity entity)
    {
        // Resources não têm operação de revoke
        return Task.FromResult(new ValidationResult());
    }
}
