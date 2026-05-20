using VianaHub.Global.Identity.Application.Dto.Request.Action;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Action;

public class UpdateActionRouteValidator : AbstractValidator<UpdateActionRequest>
{
    private readonly ILocalizationService _localization;

    public UpdateActionRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage(_localization.GetMessage("Api.Validator.Action.Update.Name.MaximumLength", 200));

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(_localization.GetMessage("Api.Validator.Action.Update.Description.MaximumLength", 255));
    }
}
