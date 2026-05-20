using VianaHub.Global.Identity.Api.Configuration;

namespace VianaHub.Global.Identity.Tests.Configuration;

public class HangfireDashboardSettingsTests
{
    [Fact(DisplayName = "HangfireDashboardSettings - Deve instanciar com valores padrão corretos")]
    [Trait("Api", "")]
    public void Instanciar_SemValores_DeveUsarValoresPadrao()
    {
        var settings = new HangfireDashboardSettings();

        Assert.True(settings.Enabled);
        Assert.True(settings.RequireBasicAuth);
        Assert.Equal("hangfire", settings.Username);
        Assert.Equal("changeme", settings.Password);
    }

    [Fact(DisplayName = "HangfireDashboardSettings - Deve atribuir e retornar valores corretamente")]
    [Trait("Api", "")]
    public void Instanciar_ComValores_DeveRetornarValoresAtribuidos()
    {
        var settings = new HangfireDashboardSettings
        {
            Enabled = false,
            RequireBasicAuth = false,
            Username = "admin",
            Password = "supersenha123"
        };

        Assert.False(settings.Enabled);
        Assert.False(settings.RequireBasicAuth);
        Assert.Equal("admin", settings.Username);
        Assert.Equal("supersenha123", settings.Password);
    }
}
