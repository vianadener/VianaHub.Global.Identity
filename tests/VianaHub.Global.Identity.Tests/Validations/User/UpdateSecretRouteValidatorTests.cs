using VianaHub.Global.Identity.Api.Validations.User;
using VianaHub.Global.Identity.Application.Dto.Request.User;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Moq;

namespace VianaHub.Global.Identity.Tests.Validations.User;

public class UpdateSecretRouteValidatorTests
{
    private readonly UpdateSecretRouteValidator _validator;

    public UpdateSecretRouteValidatorTests()
    {
        var locMock = new Mock<ILocalizationService>();
        locMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns("mensagem");
        locMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns("mensagem");

        _validator = new UpdateSecretRouteValidator(locMock.Object);
    }

    #region CurrentSecret

    [Fact(DisplayName = "Deve ser válido quando CurrentSecret e NewSecret são preenchidos corretamente")]
    [Trait("Api", "")]
    public void Validate_Sucesso_RequestValida()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "NovoSecret@1");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact(DisplayName = "Deve ser inválido quando CurrentSecret é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_CurrentSecretVazio()
    {
        var request = new UpdateSecretRequest(string.Empty, "NovoSecret@1");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.CurrentSecret));
    }

    [Fact(DisplayName = "Deve ser inválido quando CurrentSecret é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_CurrentSecretNulo()
    {
        var request = new UpdateSecretRequest(null, "NovoSecret@1");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.CurrentSecret));
    }

    #endregion

    #region NewSecret - NotEmpty

    [Fact(DisplayName = "Deve ser inválido quando NewSecret é vazio")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretVazio()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", string.Empty);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    [Fact(DisplayName = "Deve ser inválido quando NewSecret é nulo")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretNulo()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", null);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    #endregion

    #region NewSecret - MinimumLength

    [Fact(DisplayName = "Deve ser inválido quando NewSecret tem menos de 8 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretMenorQueMinimo()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "Ab@1");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    [Fact(DisplayName = "Deve ser válido quando NewSecret tem exatamente 8 caracteres válidos")]
    [Trait("Api", "")]
    public void Validate_Sucesso_NewSecretComMinimo8Caracteres()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "Ab@12345");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    #endregion

    #region NewSecret - MaximumLength

    [Fact(DisplayName = "Deve ser inválido quando NewSecret tem mais de 100 caracteres")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretMaiorQueMaximo()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "Ab@1" + new string('a', 98));

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    [Fact(DisplayName = "Deve ser válido quando NewSecret tem exatamente 100 caracteres válidos")]
    [Trait("Api", "")]
    public void Validate_Sucesso_NewSecretComMaximo100Caracteres()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "Ab@1" + new string('a', 96));

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    #endregion

    #region NewSecret - RequiresUpperCase

    [Fact(DisplayName = "Deve ser inválido quando NewSecret não contém letra maiúscula")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretSemMaiuscula()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "novosecret@1");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    #endregion

    #region NewSecret - RequiresLowerCase

    [Fact(DisplayName = "Deve ser inválido quando NewSecret não contém letra minúscula")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretSemMinuscula()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "NOVOSECRET@1");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    #endregion

    #region NewSecret - RequiresDigit

    [Fact(DisplayName = "Deve ser inválido quando NewSecret não contém número")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretSemNumero()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "NovoSecret@abc");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    #endregion

    #region NewSecret - RequiresSpecialCharacter

    [Fact(DisplayName = "Deve ser inválido quando NewSecret não contém caractere especial")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretSemCaractereEspecial()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "NovoSecret123");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    #endregion

    #region Múltiplos erros

    [Fact(DisplayName = "Deve retornar múltiplos erros quando CurrentSecret e NewSecret são inválidos")]
    [Trait("Api", "")]
    public void Validate_Insucesso_AmbosOsCamposInvalidos()
    {
        var request = new UpdateSecretRequest(string.Empty, string.Empty);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.CurrentSecret));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret));
    }

    [Fact(DisplayName = "Deve retornar múltiplos erros de NewSecret quando todas as regras são violadas")]
    [Trait("Api", "")]
    public void Validate_Insucesso_NewSecretViolaTodasAsRegras()
    {
        var request = new UpdateSecretRequest("SecretAtual@1", "abc");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count(e => e.PropertyName == nameof(UpdateSecretRequest.NewSecret)) > 1);
    }

    #endregion
}
