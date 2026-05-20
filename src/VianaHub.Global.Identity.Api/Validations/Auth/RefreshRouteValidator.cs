using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Auth;

public class RefreshRouteValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRouteValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("Application.Service.Auth.Refresh.InvalidTenantId");
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Application.Service.Auth.Refresh.TokenRequired");
    }
}
