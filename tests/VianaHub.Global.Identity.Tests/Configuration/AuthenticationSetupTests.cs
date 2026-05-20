using VianaHub.Global.Identity.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VianaHub.Global.Identity.Tests.Configuration;

public class AuthenticationSetupTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static Dictionary<string, string> DefaultValidConfig() => new()
    {
        ["JwtSettings:Issuer"] = "TestIssuer",
        ["JwtSettings:Audience"] = "TestAudience",
        ["Security:JwtMasterKey"] = "SuperSecretKeyForTestingPurposesOnly1234567890"
    };

    #region Sucesso

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve registrar autenticação sem lançar exceção com configuração válida")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_ConfiguracaoValida_NaoLancaExcecao()
    {
        var config = BuildConfiguration(DefaultValidConfig());
        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddAuthenticationConfiguration(config));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve retornar a instância de IServiceCollection")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_ConfiguracaoValida_RetornaServicesCorreto()
    {
        var config = BuildConfiguration(DefaultValidConfig());
        var services = new ServiceCollection();

        var result = services.AddAuthenticationConfiguration(config);

        Assert.Same(services, result);
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve registrar JwtSettings no container de DI")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_ConfiguracaoValida_RegistraJwtSettings()
    {
        var config = BuildConfiguration(DefaultValidConfig());
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthenticationConfiguration(config);

        var serviceTypes = services.Select(s => s.ServiceType.FullName).ToList();
        Assert.Contains(serviceTypes, s => s != null && s.Contains("IOptions"));
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve registrar esquema de autenticação JWT Bearer")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_ConfiguracaoValida_RegistraJwtBearerScheme()
    {
        var config = BuildConfiguration(DefaultValidConfig());
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthenticationConfiguration(config);

        var serviceTypes = services.Select(s => s.ServiceType.FullName).ToList();
        Assert.Contains(serviceTypes, s => s != null && s.Contains("Authentication"));
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve registrar políticas de autorização")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_ConfiguracaoValida_RegistraAuthorization()
    {
        var config = BuildConfiguration(DefaultValidConfig());
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddAuthenticationConfiguration(config);

        var serviceTypes = services.Select(s => s.ServiceType.FullName).ToList();
        Assert.Contains(serviceTypes, s => s != null && s.Contains("Authorization"));
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve aceitar configuração com ValidIssuers e ValidAudiences")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_ComMultiplosIssuersEAudiences_NaoLancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience",
            ["JwtSettings:ValidIssuers:0"] = "TestIssuer",
            ["JwtSettings:ValidIssuers:1"] = "OtherIssuer",
            ["JwtSettings:ValidAudiences:0"] = "TestAudience",
            ["JwtSettings:ValidAudiences:1"] = "OtherAudience",
            ["Security:JwtMasterKey"] = "SuperSecretKeyForTestingPurposesOnly1234567890"
        });
        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddAuthenticationConfiguration(config));

        Assert.Null(exception);
    }

    #endregion

    #region Insucesso

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve lançar exceção quando JwtSettings está ausente")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_SemJwtSettings_LancaInvalidOperationException()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Security:JwtMasterKey"] = "SuperSecretKeyForTestingPurposesOnly1234567890"
        });
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddAuthenticationConfiguration(config));
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve lançar exceção quando JwtMasterKey está ausente")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_SemJwtMasterKey_LancaInvalidOperationException()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience"
        });
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddAuthenticationConfiguration(config));
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Deve lançar exceção quando configuração está completamente vazia")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_ConfiguracaoVazia_LancaInvalidOperationException()
    {
        var config = BuildConfiguration(new Dictionary<string, string>());
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddAuthenticationConfiguration(config));
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Mensagem de exceção deve informar JwtSettings ausente")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_SemJwtSettings_MensagemExcecaoCorreta()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Security:JwtMasterKey"] = "SuperSecretKeyForTestingPurposesOnly1234567890"
        });
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() => services.AddAuthenticationConfiguration(config));

        Assert.Contains("JwtSettings", ex.Message);
    }

    [Fact(DisplayName = "AddAuthenticationConfiguration - Mensagem de exceção deve informar JwtMasterKey ausente")]
    [Trait("Api", "")]
    public void AddAuthenticationConfiguration_SemJwtMasterKey_MensagemExcecaoCorreta()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience"
        });
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() => services.AddAuthenticationConfiguration(config));

        Assert.Contains("JwtMasterKey", ex.Message);
    }

    #endregion
}
