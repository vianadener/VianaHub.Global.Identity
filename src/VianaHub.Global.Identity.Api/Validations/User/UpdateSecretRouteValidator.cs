using VianaHub.Global.Identity.Application.Dto.Request.User;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.User;

public class UpdateSecretRouteValidator : AbstractValidator<UpdateSecretRequest>
{
    private readonly ILocalizationService _localization;

    public UpdateSecretRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.CurrentSecret)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.CurrentSecret"));

        RuleFor(x => x.NewSecret)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.NewSecret"))
            .MinimumLength(8).WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.NewSecret.MinimumLength", 8))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.NewSecret.MaximumLength", 100))
            .Matches(@"[A-Z]").WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.NewSecret.RequiresUpperCase"))
            .Matches(@"[a-z]").WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.NewSecret.RequiresLowerCase"))
            .Matches(@"[0-9]").WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.NewSecret.RequiresDigit"))
            .Matches(@"[^a-zA-Z0-9]").WithMessage(_localization.GetMessage("Api.Validator.User.UpdateSecret.NewSecret.RequiresSpecialCharacter"));
    }
}
