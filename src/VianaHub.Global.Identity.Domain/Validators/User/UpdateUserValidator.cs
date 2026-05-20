using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.User;

public class UpdateUserValidator : AbstractValidator<UserEntity>
{
    public UpdateUserValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.User.InvalidId"));

        RuleFor(x => x.IsDeleted)
            .Equal(false)
            .WithMessage(localization.GetMessage("Domain.User.CannotUpdateDeleted"));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.User.NameRequired"))
            .MaximumLength(150)
            .WithMessage(localization.GetMessage("Domain.User.NameMaxLength", 150));
    }
}
