using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Auth;

public class ForgotPasswordRouteValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRouteValidator()
    {
        RuleFor(x => x.LoginIdentifier)
            .NotEmpty()
            .WithMessage("Application.Service.Auth.ForgotPassword.LoginIdentifierRequired")
            .EmailAddress()
            .WithMessage("Application.Service.Auth.ForgotPassword.InvalidEmailFormat");
    }
}
