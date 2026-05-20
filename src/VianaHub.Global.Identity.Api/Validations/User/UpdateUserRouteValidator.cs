using VianaHub.Global.Identity.Application.Dto.Request.User;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.User;

public class UpdateUserRouteValidator : AbstractValidator<UpdateUserRequest>
{
    private readonly ILocalizationService _localization;

    public UpdateUserRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage(_localization.GetMessage("Api.Validator.User.Update.Name.MaximumLength", 200));

        RuleFor(x => x.UrlImage)
            .MaximumLength(500).WithMessage(_localization.GetMessage("Api.Validator.User.Update.UrlImage.MaximumLength", 500))
            .When(x => !string.IsNullOrEmpty(x.UrlImage));
    }
}
