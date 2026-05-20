using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.App;

public class ActivateAppValidator : AbstractValidator<AppEntity>
{
    public ActivateAppValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.App.IdRequired"));

        RuleFor(x => x.IsDeleted)
            .Equal(false)
            .WithMessage(localization.GetMessage("Domain.App.CannotActivateDeleted"));

        RuleFor(x => x.IsActive)
            .Equal(false)
            .WithMessage(localization.GetMessage("Domain.App.AlreadyActive"));
    }
}
