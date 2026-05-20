using VianaHub.Global.Identity.Application.Dto.Request.Tenant;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Tenant;

public class CreateTenantRouteValidator : AbstractValidator<CreateTenantRequest>
{
    private readonly ILocalizationService _localization;

    public CreateTenantRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.Name"))
            .MaximumLength(200).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.Name.MaximumLength", 200));

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.Description"))
            .MaximumLength(500).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.Description.MaximumLength", 500));

        RuleFor(x => x.Alias)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.Alias"))
            .MaximumLength(30).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.Alias.MaximumLength", 30));

        RuleFor(x => x.UrlImage)
            .MaximumLength(500).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.UrlImage.MaximumLength", 500))
            .When(x => !string.IsNullOrEmpty(x.UrlImage));

        RuleFor(x => x.Remarks)
            .MaximumLength(1000).WithMessage(_localization.GetMessage("Api.Validator.Tenant.Create.Remarks.MaximumLength", 1000))
            .When(x => !string.IsNullOrEmpty(x.Remarks));
    }
}
