using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Auth;

public class RegisterRouteValidator : AbstractValidator<RegisterRequest>
{
    private static readonly string[] CommonPasswords = ["123456", "password", "qwerty", "12345678", "111111", "123456789", "1234567890"];

    public RegisterRouteValidator(ILocalizationService localization)
    {
        RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("Application.Service.Auth.Register.InvalidTenantId");

        RuleFor(x => x.Name).NotEmpty().WithMessage("Application.Service.Auth.Register.NameRequired");

        RuleFor(x => x.Secret)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.SecretRequired"))
            .MinimumLength(9)
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.Secret.MinimumLength", 9))
            .MaximumLength(100)
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.Secret.MaximumLength", 100))
            .Matches(@"[A-Z]")
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.Secret.RequiresUpperCase"))
            .Matches(@"[a-z]")
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.Secret.RequiresLowerCase"))
            .Matches(@"[0-9]")
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.Secret.RequiresDigit"))
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.Secret.RequiresSpecialCharacter"))
            .Must(p => !CommonPasswords.Contains(p.ToLowerInvariant()))
            .WithMessage(localization.GetMessage("Application.Service.Auth.Register.Secret.CommonPassword"));
    }
}
