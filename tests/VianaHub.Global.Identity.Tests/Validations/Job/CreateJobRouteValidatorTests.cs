using VianaHub.Global.Identity.Api.Validations.Job;
using VianaHub.Global.Identity.Application.Dto.Request.Job;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Moq;

namespace VianaHub.Global.Identity.Tests.Validations.Job;

public class CreateJobRouteValidatorTests
{
    private readonly CreateJobRouteValidator _validator;

    public CreateJobRouteValidatorTests()
    {
        var locMock = new Mock<ILocalizationService>();
        locMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("mensagem");
        locMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("mensagem");

        _validator = new CreateJobRouteValidator(locMock.Object);
    }

    private static CreateJobRequest RequestValido() => new()
    {
        JobCategory = "Categoria",
        JobName = "NomeDoJob",
        JobType = "TipoDoJob",
        JobMethod = "Execute",
        CronExpression = "0 * * * *",
        TimeZoneId = "GMT Standard Time",
        TimeoutMinutes = 5,
        Priority = 5,
        Queue = "default",
        MaxRetries = 3
    };

    #region Sucesso

    [Fact(DisplayName = "Deve ser válido quando todos os campos obrigatórios são preenchidos corretamente")]
    [Trait("Api", "")]
    public void Validate_Sucesso_RequestValida()
    {
        var result = _validator.Validate(RequestValido());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Deve ser válido quando Description é nulo (campo opcional)")]
    [Trait("Api", "")]
    public void Validate_Sucesso_DescriptionNulo()
    {
        var request = RequestValido();
        request.Description = null;

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Deve ser válido quando JobPurpose é nulo (campo opcional)")]
    [Trait("Api", "")]
    public void Validate_Sucesso_JobPurposeNulo()
    {
        var request = RequestValido();
        request.JobPurpose = null;

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Deve ser válido quando Description tem exatamente 500 caracteres")]
    [Trait("Api", "")]
    public void Validate_Sucesso_DescriptionComMaximo500Caracteres()
    {
        var request = RequestValido();
        request.Description = new string('a', 500);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Deve ser válido quando JobPurpose tem exatamente 500 caracteres")]
    [Trait("Api", "")]
    public void Validate_Sucesso_JobPurposeComMaximo500Caracteres()
    {
        var request = RequestValido();
        request.JobPurpose = new string('a', 500);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Deve ser válido quando TimeoutMinutes é zero")]
    [Trait("Api", "")]
    public void Validate_Sucesso_TimeoutMinutesZero()
    {
        var request = RequestValido();
        request.TimeoutMinutes = 0;

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Deve ser válido quando Priority é zero")]
    [Trait("Api", "")]
    public void Validate_Sucesso_PriorityZero()
    {
        var request = RequestValido();
        request.Priority = 0;

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Deve ser válido quando MaxRetries é zero")]
    [Trait("Api", "")]
    public void Validate_Sucesso_MaxRetriesZero()
    {
        var request = RequestValido();
        request.MaxRetries = 0;

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    #endregion

    #region JobCategory

    [Fact(DisplayName = "Deve ser inválido quando JobCategory é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobCategoryVazio()
    {
        var request = RequestValido();
        request.JobCategory = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobCategory));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobCategory é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobCategoryNulo()
    {
        var request = RequestValido();
        request.JobCategory = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobCategory));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobCategory tem mais de 100 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobCategoryMaiorQueMaximo()
    {
        var request = RequestValido();
        request.JobCategory = new string('a', 101);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobCategory));
    }

    #endregion

    #region JobName

    [Fact(DisplayName = "Deve ser inválido quando JobName é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobNameVazio()
    {
        var request = RequestValido();
        request.JobName = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobName));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobName é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobNameNulo()
    {
        var request = RequestValido();
        request.JobName = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobName));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobName tem mais de 150 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobNameMaiorQueMaximo()
    {
        var request = RequestValido();
        request.JobName = new string('a', 151);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobName));
    }

    #endregion

    #region Description

    [Fact(DisplayName = "Deve ser inválido quando Description tem mais de 500 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_DescriptionMaiorQueMaximo()
    {
        var request = RequestValido();
        request.Description = new string('a', 501);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.Description));
    }

    #endregion

    #region JobPurpose

    [Fact(DisplayName = "Deve ser inválido quando JobPurpose tem mais de 500 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobPurposeMaiorQueMaximo()
    {
        var request = RequestValido();
        request.JobPurpose = new string('a', 501);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobPurpose));
    }

    #endregion

    #region JobType

    [Fact(DisplayName = "Deve ser inválido quando JobType é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobTypeVazio()
    {
        var request = RequestValido();
        request.JobType = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobType));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobType é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobTypeNulo()
    {
        var request = RequestValido();
        request.JobType = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobType));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobType tem mais de 100 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobTypeMaiorQueMaximo()
    {
        var request = RequestValido();
        request.JobType = new string('a', 101);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobType));
    }

    #endregion

    #region JobMethod

    [Fact(DisplayName = "Deve ser inválido quando JobMethod é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobMethodVazio()
    {
        var request = RequestValido();
        request.JobMethod = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobMethod));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobMethod é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobMethodNulo()
    {
        var request = RequestValido();
        request.JobMethod = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobMethod));
    }

    [Fact(DisplayName = "Deve ser inválido quando JobMethod tem mais de 100 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_JobMethodMaiorQueMaximo()
    {
        var request = RequestValido();
        request.JobMethod = new string('a', 101);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobMethod));
    }

    #endregion

    #region CronExpression

    [Fact(DisplayName = "Deve ser inválido quando CronExpression é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_CronExpressionVazio()
    {
        var request = RequestValido();
        request.CronExpression = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.CronExpression));
    }

    [Fact(DisplayName = "Deve ser inválido quando CronExpression é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_CronExpressionNulo()
    {
        var request = RequestValido();
        request.CronExpression = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.CronExpression));
    }

    [Fact(DisplayName = "Deve ser inválido quando CronExpression tem mais de 100 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_CronExpressionMaiorQueMaximo()
    {
        var request = RequestValido();
        request.CronExpression = new string('a', 101);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.CronExpression));
    }

    #endregion

    #region TimeZoneId

    [Fact(DisplayName = "Deve ser inválido quando TimeZoneId é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_TimeZoneIdVazio()
    {
        var request = RequestValido();
        request.TimeZoneId = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.TimeZoneId));
    }

    [Fact(DisplayName = "Deve ser inválido quando TimeZoneId é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_TimeZoneIdNulo()
    {
        var request = RequestValido();
        request.TimeZoneId = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.TimeZoneId));
    }

    [Fact(DisplayName = "Deve ser inválido quando TimeZoneId tem mais de 100 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_TimeZoneIdMaiorQueMaximo()
    {
        var request = RequestValido();
        request.TimeZoneId = new string('a', 101);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.TimeZoneId));
    }

    #endregion

    #region TimeoutMinutes

    [Fact(DisplayName = "Deve ser inválido quando TimeoutMinutes é negativo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_TimeoutMinutesNegativo()
    {
        var request = RequestValido();
        request.TimeoutMinutes = -1;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.TimeoutMinutes));
    }

    #endregion

    #region Priority

    [Fact(DisplayName = "Deve ser inválido quando Priority é negativo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_PriorityNegativo()
    {
        var request = RequestValido();
        request.Priority = -1;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.Priority));
    }

    #endregion

    #region Queue

    [Fact(DisplayName = "Deve ser inválido quando Queue é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_QueueVazio()
    {
        var request = RequestValido();
        request.Queue = string.Empty;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.Queue));
    }

    [Fact(DisplayName = "Deve ser inválido quando Queue é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_QueueNulo()
    {
        var request = RequestValido();
        request.Queue = null;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.Queue));
    }

    [Fact(DisplayName = "Deve ser inválido quando Queue tem mais de 100 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_QueueMaiorQueMaximo()
    {
        var request = RequestValido();
        request.Queue = new string('a', 101);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.Queue));
    }

    #endregion

    #region MaxRetries

    [Fact(DisplayName = "Deve ser inválido quando MaxRetries é negativo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_MaxRetriesNegativo()
    {
        var request = RequestValido();
        request.MaxRetries = -1;

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.MaxRetries));
    }

    #endregion

    #region Múltiplos erros

    [Fact(DisplayName = "Deve retornar múltiplos erros quando todos os campos obrigatórios são inválidos")]
    [Trait("Api", "")]
    public void Validate_Insucesso_TodosOsCamposObrigatoriosInvalidos()
    {
        var request = new CreateJobRequest
        {
            JobCategory = string.Empty,
            JobName = string.Empty,
            JobType = string.Empty,
            JobMethod = string.Empty,
            CronExpression = string.Empty,
            TimeZoneId = string.Empty,
            Queue = string.Empty,
            TimeoutMinutes = -1,
            Priority = -1,
            MaxRetries = -1
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobCategory));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobName));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobType));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.JobMethod));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.CronExpression));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.TimeZoneId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.Queue));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.TimeoutMinutes));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.Priority));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateJobRequest.MaxRetries));
    }

    #endregion
}
