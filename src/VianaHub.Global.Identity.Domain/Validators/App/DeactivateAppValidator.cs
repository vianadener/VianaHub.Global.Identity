using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.App;

public class DeactivateAppValidator : AbstractValidator<AppEntity>
{
    public DeactivateAppValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.App.IdRequired"));

        RuleFor(x => x.IsDeleted)
            .Equal(false)
            .WithMessage(localization.GetMessage("Domain.App.CannotDeactivateDeleted"));

        RuleFor(x => x.IsActive)
            .Equal(true)
            .WithMessage(localization.GetMessage("Domain.App.AlreadyInactive"));
    }
}
