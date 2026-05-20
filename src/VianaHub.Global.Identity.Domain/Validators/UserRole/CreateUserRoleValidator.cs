using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.UserRole;

public class CreateUserRoleValidator : AbstractValidator<UserRoleEntity>
{
    public CreateUserRoleValidator(ILocalizationService localization)
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.UserRole.TenantIdRequired"));

        RuleFor(x => x.AppId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.UserRole.AppIdRequired"));

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.UserRole.UserIdRequired"));

        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.UserRole.RoleIdRequired"));
    }
}
