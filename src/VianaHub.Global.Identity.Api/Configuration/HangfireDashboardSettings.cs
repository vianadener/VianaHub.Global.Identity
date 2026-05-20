namespace VianaHub.Global.Identity.Api.Configuration;

public class HangfireDashboardSettings
{
    /// <summary>
    /// Habilita o dashboard do Hangfire.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Habilita autenticação Basic Auth no dashboard.
    /// Quando false e o ambiente não é Development, o acesso é bloqueado.
    /// </summary>
    public bool RequireBasicAuth { get; set; } = true;

    /// <summary>
    /// Utilizador para Basic Auth.
    /// </summary>
    public string Username { get; set; } = "hangfire";

    /// <summary>
    /// Password para Basic Auth.
    /// </summary>
    public string Password { get; set; } = "changeme";
}
