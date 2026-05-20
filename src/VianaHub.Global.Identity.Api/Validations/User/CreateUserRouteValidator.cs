using VianaHub.Global.Identity.Application.Dto.Request.User;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.User;

public class CreateUserRouteValidator : AbstractValidator<CreateUserRequest>
{
    private readonly ILocalizationService _localization;

    public CreateUserRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.User.Create.Name"))
            .MaximumLength(200).WithMessage(_localization.GetMessage("Api.Validator.User.Create.Name.MaximumLength", 200));

        RuleFor(x => x.Secret)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.User.Create.Secret"))
            .MinimumLength(8).WithMessage(_localization.GetMessage("Api.Validator.User.Create.Secret.MinimumLength", 8))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.User.Create.Secret.MaximumLength", 100))
            .Matches(@"[A-Z]").WithMessage(_localization.GetMessage("Api.Validator.User.Create.Secret.RequiresUpperCase"))
            .Matches(@"[a-z]").WithMessage(_localization.GetMessage("Api.Validator.User.Create.Secret.RequiresLowerCase"))
            .Matches(@"[0-9]").WithMessage(_localization.GetMessage("Api.Validator.User.Create.Secret.RequiresDigit"))
            .Matches(@"[^a-zA-Z0-9]").WithMessage(_localization.GetMessage("Api.Validator.User.Create.Secret.RequiresSpecialCharacter"));

        RuleFor(x => x.ConfirmSecret)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.User.Create.ConfirmSecret"))
            .Equal(x => x.Secret).WithMessage(_localization.GetMessage("Api.Validator.User.Create.ConfirmSecret.Equal"));
    }
}
