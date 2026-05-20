using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Auth;

public class ResetPasswordRouteValidator : AbstractValidator<ResetPasswordRequest>
{
    private static readonly string[] CommonPasswords = ["123456", "password", "qwerty", "12345678", "111111", "123456789", "1234567890"];

    public ResetPasswordRouteValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.TokenRequired"));

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPasswordRequired"))
            .MinimumLength(9)
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPassword.MinimumLength", 9))
            .MaximumLength(100)
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPassword.MaximumLength", 100))
            .Matches(@"[A-Z]")
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPassword.RequiresUpperCase"))
            .Matches(@"[a-z]")
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPassword.RequiresLowerCase"))
            .Matches(@"[0-9]")
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPassword.RequiresDigit"))
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPassword.RequiresSpecialCharacter"))
            .Must(p => !CommonPasswords.Contains(p.ToLowerInvariant()))
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.NewPassword.CommonPassword"));

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.ConfirmPasswordRequired"))
            .Equal(x => x.NewPassword)
            .WithMessage(localization.GetMessage("Api.Validator.Auth.ResetPassword.PasswordsMustMatch"));
    }
}
