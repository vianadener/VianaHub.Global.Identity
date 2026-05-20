using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Auth;

public class LoginRouteValidator : AbstractValidator<LoginRequest>
{
    public LoginRouteValidator()
    {
        RuleFor(x => x.LoginIdentifier)
            .NotEmpty()
            .WithMessage("Application.Service.Auth.Login.LoginIdentifierRequired");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Application.Service.Auth.Login.PasswordRequired");
    }
}
