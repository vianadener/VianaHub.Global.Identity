using VianaHub.Global.Identity.Api.Configuration;

namespace VianaHub.Global.Identity.Tests.Configuration;

public class ApplicationSettingsTests
{
    [Fact(DisplayName = "ApplicationSettings - Deve instanciar com valores padrão nulos")]
    [Trait("Api", "")]
    public void Instanciar_SemValores_PropriedadesNulas()
    {
        var settings = new ApplicationSettings();

        Assert.Null(settings.Application);
        Assert.Null(settings.Name);
        Assert.Null(settings.Contact);
        Assert.Null(settings.Email);
        Assert.Null(settings.Environment);
    }

    [Fact(DisplayName = "ApplicationSettings - Deve atribuir e retornar valores corretamente")]
    [Trait("Api", "")]
    public void Instanciar_ComValores_DeveRetornarValoresAtribuidos()
    {
        var settings = new ApplicationSettings
        {
            Application = "Identity",
            Name = "Identity API",
            Contact = "Admin",
            Email = "admin@test.com",
            Environment = "Development"
        };

        Assert.Equal("Identity", settings.Application);
        Assert.Equal("Identity API", settings.Name);
        Assert.Equal("Admin", settings.Contact);
        Assert.Equal("admin@test.com", settings.Email);
        Assert.Equal("Development", settings.Environment);
    }
}
