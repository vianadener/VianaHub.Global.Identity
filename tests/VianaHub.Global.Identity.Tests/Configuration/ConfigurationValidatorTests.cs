using VianaHub.Global.Identity.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;

namespace VianaHub.Global.Identity.Tests.Configuration;

public class ConfigurationValidatorTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static IWebHostEnvironment BuildEnvironment(string environmentName)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns(environmentName);
        return mock.Object;
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve passar quando todas as configurações são válidas")]
    [Trait("Api", "")]
    public void ValidateConfiguration_ConfiguracoesValidas_NaoLancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        });

        var env = BuildEnvironment("Development");

        var exception = Record.Exception(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando ConnectionString está ausente")]
    [Trait("Api", "")]
    public void ValidateConfiguration_SemConnectionString_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.ConnectionStrings.DefaultConnectionMissing", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando EncryptionKey está ausente")]
    [Trait("Api", "")]
    public void ValidateConfiguration_SemEncryptionKey_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Jwt.EncryptionKeyMissing", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando EncryptionKey é menor que 32 caracteres")]
    [Trait("Api", "")]
    public void ValidateConfiguration_EncryptionKeyCurta_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "curta",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Jwt.EncryptionKeyTooShort", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando Issuer está ausente")]
    [Trait("Api", "")]
    public void ValidateConfiguration_SemIssuer_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Jwt.IssuerMissing", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando Audience está ausente")]
    [Trait("Api", "")]
    public void ValidateConfiguration_SemAudience_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Jwt.AudienceMissing", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando AccessTokenExpiration é zero")]
    [Trait("Api", "")]
    public void ValidateConfiguration_AccessTokenExpirationZero_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "0",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Jwt.AccessTokenExpirationInvalid", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando RefreshTokenExpiration é zero")]
    [Trait("Api", "")]
    public void ValidateConfiguration_RefreshTokenExpirationZero_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "0"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Jwt.RefreshTokenExpirationInvalid", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando RateLimiting está habilitado sem PolicyName")]
    [Trait("Api", "")]
    public void ValidateConfiguration_RateLimitingHabilitadoSemPolicy_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            ["RateLimiting:EnableRateLimiting"] = "true"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.RateLimiting.PolicyMissing", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando Swagger está habilitado em produção")]
    [Trait("Api", "")]
    public void ValidateConfiguration_SwaggerHabilitadoEmProducao_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            ["Swagger:Enabled"] = "true"
        });

        var env = BuildEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Security.SwaggerEnabledInProduction", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando Hangfire está habilitado em produção")]
    [Trait("Api", "")]
    public void ValidateConfiguration_HangfireHabilitadoEmProducao_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            ["HangfireDashboard:Enabled"] = "true"
        });

        var env = BuildEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Security.HangfireEnabledInProduction", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando Hangfire usa senha fraca em produção")]
    [Trait("Api", "")]
    public void ValidateConfiguration_HangfireSenhaFracaEmProducao_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            ["HangfireDashboard:RequireBasicAuth"] = "true",
            ["HangfireDashboard:Password"] = "changeme"
        });

        var env = BuildEnvironment("Production");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Security.HangfireWeakPassword", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando múltiplas configurações estão ausentes")]
    [Trait("Api", "")]
    public void ValidateConfiguration_MultiplasFalhas_LancaExcecaoComTodosErros()
    {
        var config = BuildConfiguration(new Dictionary<string, string>());
        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.ConnectionStrings.DefaultConnectionMissing", exception.Message);
        Assert.Contains("Config.Jwt.EncryptionKeyMissing", exception.Message);
        Assert.Contains("Config.Jwt.IssuerMissing", exception.Message);
        Assert.Contains("Config.Jwt.AudienceMissing", exception.Message);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve passar com CORS habilitado e PolicyName definido")]
    [Trait("Api", "")]
    public void ValidateConfiguration_CorsHabilitadoComPolicy_NaoLancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            ["Cors:EnableCors"] = "true",
            ["Cors:PolicyName"] = "MyPolicy"
        });

        var env = BuildEnvironment("Development");

        var exception = Record.Exception(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "ValidateConfiguration - Deve lançar exceção quando CORS habilitado sem PolicyName")]
    [Trait("Api", "")]
    public void ValidateConfiguration_CorsHabilitadoSemPolicy_LancaExcecao()
    {
        var config = BuildConfiguration(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=.;Database=test;",
            ["JwtKeyManagement:EncryptionKey"] = "chave-muito-segura-com-32-chars!!",
            ["JwtSettings:Issuer"] = "identity-api",
            ["JwtSettings:Audience"] = "identity-client",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7",
            ["Cors:EnableCors"] = "true"
        });

        var env = BuildEnvironment("Development");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ConfigurationValidator.ValidateConfiguration(config, env));

        Assert.Contains("Config.Cors.PolicyNameMissing", exception.Message);
    }
}
