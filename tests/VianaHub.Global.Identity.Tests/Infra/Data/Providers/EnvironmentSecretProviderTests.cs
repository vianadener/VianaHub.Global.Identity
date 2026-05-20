using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Data.Providers;
using Microsoft.Extensions.Configuration;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Providers;

public class EnvironmentSecretProviderTests
{
    private readonly Mock<IConfiguration> _configurationMock = new();

    private EnvironmentSecretProvider CreateSut() => new(_configurationMock.Object);

    private static void SetEnvironmentVariable(string? value)
        => Environment.SetEnvironmentVariable("JWT_MASTER_KEY", value);

    #region Construtor

    [Fact(DisplayName = "EnvironmentSecretProvider - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "EnvironmentSecretProvider - Deve implementar ISecretProvider")]
    [Trait("Infra.Data", "")]
    public void EnvironmentSecretProvider_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<ISecretProvider>(sut);
    }

    #endregion

    #region GetMasterKey - Sucesso via variável de ambiente

    [Fact(DisplayName = "GetMasterKey - Deve retornar chave da variável de ambiente JWT_MASTER_KEY")]
    [Trait("Infra.Data", "")]
    public void GetMasterKey_VariavelDeAmbienteDefinida_DeveRetornarChave()
    {
        SetEnvironmentVariable("chave-via-env");
        var sut = CreateSut();

        try
        {
            var result = sut.GetMasterKey();

            Assert.Equal("chave-via-env", result);
        }
        finally
        {
            SetEnvironmentVariable(null);
        }
    }

    [Fact(DisplayName = "GetMasterKey - Deve priorizar variável de ambiente sobre configuração")]
    [Trait("Infra.Data", "")]
    public void GetMasterKey_AmbosDefinidos_DevePriorizarVariavelDeAmbiente()
    {
        SetEnvironmentVariable("chave-env");
        _configurationMock.Setup(x => x["Security:JwtMasterKey"]).Returns("chave-config");
        var sut = CreateSut();

        try
        {
            var result = sut.GetMasterKey();

            Assert.Equal("chave-env", result);
        }
        finally
        {
            SetEnvironmentVariable(null);
        }
    }

    #endregion

    #region GetMasterKey - Sucesso via configuração Security:JwtMasterKey

    [Fact(DisplayName = "GetMasterKey - Deve retornar chave de 'Security:JwtMasterKey' quando variável de ambiente ausente")]
    [Trait("Infra.Data", "")]
    public void GetMasterKey_SemEnvComSecurityConfig_DeveRetornarChaveDeConfig()
    {
        SetEnvironmentVariable(null);
        _configurationMock.Setup(x => x["Security:JwtMasterKey"]).Returns("chave-security-config");
        _configurationMock.Setup(x => x["JwtMasterKey"]).Returns((string?)null);
        var sut = CreateSut();

        var result = sut.GetMasterKey();

        Assert.Equal("chave-security-config", result);
    }

    #endregion

    #region GetMasterKey - Sucesso via configuração JwtMasterKey

    [Fact(DisplayName = "GetMasterKey - Deve retornar chave de 'JwtMasterKey' quando variável de ambiente e Security:JwtMasterKey ausentes")]
    [Trait("Infra.Data", "")]
    public void GetMasterKey_SemEnvSemSecurityComJwtConfig_DeveRetornarChaveDeJwtConfig()
    {
        SetEnvironmentVariable(null);
        _configurationMock.Setup(x => x["Security:JwtMasterKey"]).Returns((string?)null);
        _configurationMock.Setup(x => x["JwtMasterKey"]).Returns("chave-jwt-config");
        var sut = CreateSut();

        var result = sut.GetMasterKey();

        Assert.Equal("chave-jwt-config", result);
    }

    #endregion

    #region GetMasterKey - Insucesso: nenhuma fonte configurada

    [Fact(DisplayName = "GetMasterKey - Deve lançar InvalidOperationException quando nenhuma fonte está configurada")]
    [Trait("Infra.Data", "")]
    public void GetMasterKey_NenhumaFonteConfigurada_DeveLancarInvalidOperationException()
    {
        SetEnvironmentVariable(null);
        _configurationMock.Setup(x => x["Security:JwtMasterKey"]).Returns((string?)null);
        _configurationMock.Setup(x => x["JwtMasterKey"]).Returns((string?)null);
        var sut = CreateSut();

        var exception = Assert.Throws<InvalidOperationException>(() => sut.GetMasterKey());

        Assert.Contains("JWT Master Key não configurada", exception.Message);
    }

    [Fact(DisplayName = "GetMasterKey - Deve lançar InvalidOperationException quando variável de ambiente está vazia")]
    [Trait("Infra.Data", "")]
    public void GetMasterKey_VariavelDeAmbienteVazia_DeveLancarInvalidOperationException()
    {
        SetEnvironmentVariable(string.Empty);
        _configurationMock.Setup(x => x["Security:JwtMasterKey"]).Returns((string?)null);
        _configurationMock.Setup(x => x["JwtMasterKey"]).Returns((string?)null);
        var sut = CreateSut();

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() => sut.GetMasterKey());

            Assert.Contains("JWT Master Key não configurada", exception.Message);
        }
        finally
        {
            SetEnvironmentVariable(null);
        }
    }

    [Fact(DisplayName = "GetMasterKey - Mensagem de exceção deve orientar sobre as formas de configuração")]
    [Trait("Infra.Data", "")]
    public void GetMasterKey_NenhumaFonteConfigurada_MensagemDeveOrientarConfiguracoes()
    {
        SetEnvironmentVariable(null);
        _configurationMock.Setup(x => x["Security:JwtMasterKey"]).Returns((string?)null);
        _configurationMock.Setup(x => x["JwtMasterKey"]).Returns((string?)null);
        var sut = CreateSut();

        var exception = Assert.Throws<InvalidOperationException>(() => sut.GetMasterKey());

        Assert.Contains("JWT_MASTER_KEY", exception.Message);
        Assert.Contains("Security:JwtMasterKey", exception.Message);
    }

    #endregion
}
