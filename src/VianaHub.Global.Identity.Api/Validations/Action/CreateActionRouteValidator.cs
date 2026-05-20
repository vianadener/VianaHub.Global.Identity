using VianaHub.Global.Identity.Application.Dto.Request.Action;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Action;

public class CreateActionRouteValidator : AbstractValidator<CreateActionRequest>
{
    private readonly ILocalizationService _localization;

    public CreateActionRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Action.Create.Name"))
            .MaximumLength(50).WithMessage(_localization.GetMessage("Api.Validator.Action.Create.Name.MaximumLength", 50));

        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage(_localization.GetMessage("Api.Validator.Action.Create.Description.MaximumLength", 255));

    }
}
