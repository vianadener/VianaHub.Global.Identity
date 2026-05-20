using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.Action;

public class CreateActionValidator : AbstractValidator<ActionEntity>
{
    public CreateActionValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Action.NameRequired"))
            .MaximumLength(50)
            .WithMessage(localization.GetMessage("Domain.Action.NameMaxLength", 50));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Action.DescriptionRequired"))
            .MaximumLength(255)
            .WithMessage(localization.GetMessage("Domain.Action.DescriptionMaxLength", 255));
    }
}
