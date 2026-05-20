using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.User;

public class CreateUserValidator : AbstractValidator<UserEntity>
{
    public CreateUserValidator(ILocalizationService localization)
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.User.TenantIdRequired"));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Domain.User.NameRequired"))
            .MaximumLength(150)
            .WithMessage(localization.GetMessage("Domain.User.NameMaxLength", 150));


        RuleFor(x => x.PasswordHash)
            .NotNull()
            .WithMessage(localization.GetMessage("Domain.User.SecretHashRequired"))
            .MinimumLength(60)
            .WithMessage(localization.GetMessage("Domain.User.SecretHashInvalid"))
            .MaximumLength(500)
            .WithMessage(localization.GetMessage("Domain.User.SecretHashInvalid"));
    }
}
