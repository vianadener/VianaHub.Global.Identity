using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Domain.Validators.RolePermission;

public class RolePermissionValidator : AbstractValidator<RolePermissionEntity>
{
    public RolePermissionValidator(ILocalizationService localization)
    {
        RuleFor(x => x.TenantId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.RolePermission.TenantIdRequired"));

        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.RolePermission.RoleIdRequired"));

        RuleFor(x => x.ResourceId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.RolePermission.ResourceIdRequired"));

        RuleFor(x => x.ActionId)
            .GreaterThan(0)
            .WithMessage(localization.GetMessage("Domain.RolePermission.ActionIdRequired"));
    }
}
