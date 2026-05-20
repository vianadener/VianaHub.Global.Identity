using VianaHub.Global.Identity.Application.Dto.Request.Job;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Validations.Job;

public class CreateJobRouteValidator : AbstractValidator<CreateJobRequest>
{
    private readonly ILocalizationService _localization;

    public CreateJobRouteValidator(ILocalizationService localization)
    {
        _localization = localization;

        RuleFor(x => x.JobCategory)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobCategory"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobCategory.MaximumLength", 100));

        RuleFor(x => x.JobName)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobName"))
            .MaximumLength(150).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobName.MaximumLength", 150));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.Description.MaximumLength", 500))
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.JobPurpose)
            .MaximumLength(500).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobPurpose.MaximumLength", 500))
            .When(x => !string.IsNullOrEmpty(x.JobPurpose));

        RuleFor(x => x.JobType)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobType"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobType.MaximumLength", 100));

        RuleFor(x => x.JobMethod)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobMethod"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.JobMethod.MaximumLength", 100));

        RuleFor(x => x.CronExpression)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Job.Create.CronExpression"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.CronExpression.MaximumLength", 100));

        RuleFor(x => x.TimeZoneId)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Job.Create.TimeZoneId"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.TimeZoneId.MaximumLength", 100));

        RuleFor(x => x.TimeoutMinutes)
            .GreaterThanOrEqualTo(0).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.TimeoutMinutes.GreaterThanOrEqualTo", 0));

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.Priority.GreaterThanOrEqualTo", 0));

        RuleFor(x => x.Queue)
            .NotEmpty().WithMessage(_localization.GetMessage("Api.Validator.Job.Create.Queue"))
            .MaximumLength(100).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.Queue.MaximumLength", 100));

        RuleFor(x => x.MaxRetries)
            .GreaterThanOrEqualTo(0).WithMessage(_localization.GetMessage("Api.Validator.Job.Create.MaxRetries.GreaterThanOrEqualTo", 0));
    }
}
