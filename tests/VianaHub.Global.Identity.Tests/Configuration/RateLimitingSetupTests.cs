using VianaHub.Global.Identity.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VianaHub.Global.Identity.Tests.Configuration;

public class RateLimitingSetupTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    [Fact(DisplayName = "AddRateLimitingConfiguration - Deve registrar rate limiting com configurações OldestFirst")]
    [Trait("Api", "")]
    public void AddRateLimitingConfiguration_ConfiguracoesOldestFirst_NaoLancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["RateLimiting:PermitLimit"] = "100",
            ["RateLimiting:WindowMinutes"] = "1",
            ["RateLimiting:QueueLimit"] = "10",
            ["RateLimiting:QueueProcessingOrder"] = "OldestFirst"
        });

        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddRateLimitingConfiguration(config));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddRateLimitingConfiguration - Deve registrar rate limiting com QueueProcessingOrder NewestFirst")]
    [Trait("Api", "")]
    public void AddRateLimitingConfiguration_ConfiguracoesNewestFirst_NaoLancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["RateLimiting:PermitLimit"] = "50",
            ["RateLimiting:WindowMinutes"] = "5",
            ["RateLimiting:QueueLimit"] = "5",
            ["RateLimiting:QueueProcessingOrder"] = "NewestFirst"
        });

        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddRateLimitingConfiguration(config));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddRateLimitingConfiguration - Deve usar OldestFirst quando QueueProcessingOrder não informado")]
    [Trait("Api", "")]
    public void AddRateLimitingConfiguration_SemQueueProcessingOrder_UsaOldestFirst()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["RateLimiting:PermitLimit"] = "100",
            ["RateLimiting:WindowMinutes"] = "1",
            ["RateLimiting:QueueLimit"] = "10"
        });

        var services = new ServiceCollection();

        var exception = Record.Exception(() => services.AddRateLimitingConfiguration(config));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "AddRateLimitingConfiguration - Deve retornar a própria instância de IServiceCollection")]
    [Trait("Api", "")]
    public void AddRateLimitingConfiguration_Chamado_RetornaServicesEncadeamento()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["RateLimiting:PermitLimit"] = "100",
            ["RateLimiting:WindowMinutes"] = "1",
            ["RateLimiting:QueueLimit"] = "10"
        });

        var services = new ServiceCollection();
        var result = services.AddRateLimitingConfiguration(config);

        Assert.Same(services, result);
    }
}
