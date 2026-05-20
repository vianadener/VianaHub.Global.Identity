using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.Action;

public class DeleteActionValidator : AbstractValidator<ActionEntity>
{
    public DeleteActionValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.Action.IdRequired"));

        RuleFor(x => x.IsDeleted)
            .Must(x => !x)
            .WithMessage(localization.GetMessage("Domain.Action.AlreadyDeleted"));
    }
}
