using VianaHub.Global.Identity.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VianaHub.Global.Identity.Tests.Configuration;

public class CorsSetupTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    [Fact(DisplayName = "AddCorsConfiguration - Deve registrar CORS quando habilitado com origens específicas")]
    [Trait("Api", "")]
    public void AddCorsConfiguration_HabilitadoComOrigensEspecificas_NaoLancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Cors:EnableCors"] = "true",
            ["Cors:PolicyName"] = "TestPolicy",
            ["Cors:AllowedOrigins:0"] = "https://example.com",
            ["Cors:AllowedMethods:0"] = "GET",
            ["Cors:AllowedHeaders:0"] = "Content-Type",
            ["Cors:AllowCredentials"] = "true",
            ["Cors:MaxAge"] = "600"
        });

        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddCorsConfiguration(config));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddCorsConfiguration - Deve retornar services sem registrar CORS quando desabilitado")]
    [Trait("Api", "")]
    public void AddCorsConfiguration_Desabilitado_RetornaServicesIntactos()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Cors:EnableCors"] = "false"
        });

        var services = new ServiceCollection();
        var result = services.AddCorsConfiguration(config);

        Assert.Same(services, result);
        Assert.DoesNotContain(result, s => s.ServiceType.Name.Contains("CorsOptions"));
    }

    [Fact(DisplayName = "AddCorsConfiguration - Deve lançar exceção quando ambiente é Produção com origem wildcard")]
    [Trait("Api", "")]
    public void AddCorsConfiguration_ProducaoComWildcard_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Cors:EnableCors"] = "true",
            ["Cors:PolicyName"] = "TestPolicy",
            ["Cors:AllowedOrigins:0"] = "*",
            ["ASPNETCORE_ENVIRONMENT"] = "Production"
        });

        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddCorsConfiguration(config));
    }

    [Fact(DisplayName = "AddCorsConfiguration - Deve registrar CORS com origens wildcard em Development")]
    [Trait("Api", "")]
    public void AddCorsConfiguration_DevelopmentComWildcard_NaoLancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Cors:EnableCors"] = "true",
            ["Cors:PolicyName"] = "TestPolicy",
            ["Cors:AllowedOrigins:0"] = "*",
            ["ASPNETCORE_ENVIRONMENT"] = "Development"
        });

        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddCorsConfiguration(config));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddCorsConfiguration - Deve usar PolicyName padrão quando não configurado")]
    [Trait("Api", "")]
    public void AddCorsConfiguration_SemPolicyName_UsaValorPadrao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Cors:EnableCors"] = "true",
            ["Cors:AllowedOrigins:0"] = "https://example.com"
        });

        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddCorsConfiguration(config));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddCorsConfiguration - Deve registrar CORS com array de origens vazio (AllowAnyOrigin em não-produção)")]
    [Trait("Api", "")]
    public void AddCorsConfiguration_SemOrigensDefinidas_UsaAllowAnyOriginEmDevelopment()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["Cors:EnableCors"] = "true",
            ["Cors:PolicyName"] = "TestPolicy",
            ["ASPNETCORE_ENVIRONMENT"] = "Development"
        });

        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddCorsConfiguration(config));

        Assert.Null(exception);
    }
}
