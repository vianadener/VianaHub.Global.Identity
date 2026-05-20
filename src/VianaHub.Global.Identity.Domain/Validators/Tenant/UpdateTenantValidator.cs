using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.Tenant;

public class UpdateTenantValidator : AbstractValidator<TenantEntity>
{
    public UpdateTenantValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.Tenant.InvalidId"));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Tenant.NameRequired"))
            .MaximumLength(200)
            .WithMessage(localization.GetMessage("Domain.Tenant.NameMaxLength", 200));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Tenant.DescriptionRequired"))
            .MaximumLength(500)
            .WithMessage(localization.GetMessage("Domain.Tenant.DescriptionMaxLength", 500));

        RuleFor(x => x.Alias)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Tenant.AliasRequired"))
            .MaximumLength(30)
            .WithMessage(localization.GetMessage("Domain.Tenant.AliasMaxLength", 30));

        RuleFor(x => x.IsDeleted)
            .Equal(false)
            .WithMessage(localization.GetMessage("Domain.Tenant.CannotUpdateDeleted"));
    }
}
