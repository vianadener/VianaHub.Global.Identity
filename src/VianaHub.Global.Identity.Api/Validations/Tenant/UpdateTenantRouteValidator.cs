using VianaHub.Global.Identity.Application.Dto.Request.Tenant;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Tenant;

public class UpdateTenantRouteValidator : AbstractValidator<UpdateTenantRequest>
{
    private readonly ILocalizationService _localization;

    public UpdateTenantRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.Name"))
            .MaximumLength(200).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.Name.MaximumLength", 200));

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.Description"))
            .MaximumLength(500).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.Description.MaximumLength", 500));

        RuleFor(x => x.Alias)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.Alias"))
            .MaximumLength(30).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.Alias.MaximumLength", 30));

        RuleFor(x => x.UrlImage)
            .MaximumLength(500).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.UrlImage.MaximumLength", 500))
            .When(x => !string.IsNullOrEmpty(x.UrlImage));

        RuleFor(x => x.Remarks)
            .MaximumLength(1000).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Update.Remarks.MaximumLength", 1000))
            .When(x => !string.IsNullOrEmpty(x.Remarks));
    }
}
