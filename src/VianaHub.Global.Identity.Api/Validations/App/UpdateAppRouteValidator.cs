using VianaHub.Global.Identity.Application.Dto.Request.App;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.App;

public class UpdateAppRouteValidator : AbstractValidator<UpdateAppRequest>
{
    public UpdateAppRouteValidator(ILocalizationService localization)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Api.Validation.App.NameRequired"))
            .MaximumLength(200)
            .WithMessage(localization.GetMessage("Api.Validation.App.NameMaxLength", 200));

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(localization.GetMessage("Api.Validation.App.DescriptionRequired"))
            .MaximumLength(500)
            .WithMessage(localization.GetMessage("Api.Validation.App.DescriptionMaxLength", 500));
    }
}
