using VianaHub.Global.Identity.Api.Configuration.Swagger;

namespace VianaHub.Global.Identity.Tests.Configuration.Swagger;

public class SwaggerSettingsTests
{
    [Fact(DisplayName = "SwaggerSettings - Deve instanciar com valores padrão")]
    [Trait("Api", "")]
    public void Instanciar_SemValores_DeveUsarValoresPadrao()
    {
        var settings = new SwaggerSettings();

        Assert.Null(settings.Title);
        Assert.Null(settings.Description);
        Assert.Null(settings.EnvironmentBadge);
        Assert.True(settings.ShowEnvironment);
        Assert.Null(settings.Contact);
        Assert.Null(settings.License);
    }

    [Fact(DisplayName = "SwaggerSettings - Deve atribuir e retornar valores corretamente")]
    [Trait("Api", "")]
    public void Instanciar_ComValores_DeveRetornarValoresAtribuidos()
    {
        var settings = new SwaggerSettings
        {
            Title = "Identity API",
            Description = "API de identidade",
            EnvironmentBadge = "🔧 Development",
            ShowEnvironment = false,
            Contact = new SwaggerContact { Name = "Dev", Email = "dev@test.com", Url = "https://test.com" },
            License = new SwaggerLicense { Name = "MIT", Url = "https://mit.org" }
        };

        Assert.Equal("Identity API", settings.Title);
        Assert.Equal("API de identidade", settings.Description);
        Assert.Equal("🔧 Development", settings.EnvironmentBadge);
        Assert.False(settings.ShowEnvironment);
        Assert.NotNull(settings.Contact);
        Assert.Equal("Dev", settings.Contact.Name);
        Assert.NotNull(settings.License);
        Assert.Equal("MIT", settings.License.Name);
    }
}

public class SwaggerContactTests
{
    [Fact(DisplayName = "SwaggerContact - Deve instanciar com valores nulos")]
    [Trait("Api", "")]
    public void Instanciar_SemValores_PropriedadesNulas()
    {
        var contact = new SwaggerContact();

        Assert.Null(contact.Name);
        Assert.Null(contact.Email);
        Assert.Null(contact.Url);
    }

    [Fact(DisplayName = "SwaggerContact - Deve atribuir e retornar valores corretamente")]
    [Trait("Api", "")]
    public void Instanciar_ComValores_DeveRetornarValoresAtribuidos()
    {
        var contact = new SwaggerContact
        {
            Name = "Suporte",
            Email = "suporte@empresa.com",
            Url = "https://empresa.com"
        };

        Assert.Equal("Suporte", contact.Name);
        Assert.Equal("suporte@empresa.com", contact.Email);
        Assert.Equal("https://empresa.com", contact.Url);
    }
}

public class SwaggerLicenseTests
{
    [Fact(DisplayName = "SwaggerLicense - Deve instanciar com valores nulos")]
    [Trait("Api", "")]
    public void Instanciar_SemValores_PropriedadesNulas()
    {
        var license = new SwaggerLicense();

        Assert.Null(license.Name);
        Assert.Null(license.Url);
    }

    [Fact(DisplayName = "SwaggerLicense - Deve atribuir e retornar valores corretamente")]
    [Trait("Api", "")]
    public void Instanciar_ComValores_DeveRetornarValoresAtribuidos()
    {
        var license = new SwaggerLicense
        {
            Name = "Apache 2.0",
            Url = "https://apache.org/licenses/LICENSE-2.0"
        };

        Assert.Equal("Apache 2.0", license.Name);
        Assert.Equal("https://apache.org/licenses/LICENSE-2.0", license.Url);
    }
}
