using VianaHub.Global.Identity.Api.Configuration;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;
using System.Text;

namespace VianaHub.Global.Identity.Api.Filters;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly HangfireDashboardSettings _settings;
    private readonly ILogger<HangfireDashboardAuthorizationFilter> _logger;

    public HangfireDashboardAuthorizationFilter(
        IOptions<HangfireDashboardSettings> settings,
        ILogger<HangfireDashboardAuthorizationFilter> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        var isDebugDevelopment = env != null && env.IsDevelopment() && System.Diagnostics.Debugger.IsAttached;

        // Em Development com debugger anexado, libera sem autenticação (apenas local)
        if (isDebugDevelopment)
        {
            _logger.LogDebug("[Hangfire] Acesso ao dashboard liberado: ambiente Development com debugger.");
            return true;
        }

        // Se Basic Auth não está configurado, bloqueia por segurança
        if (!_settings.RequireBasicAuth)
        {
            _logger.LogWarning("[Hangfire] Acesso ao dashboard bloqueado: RequireBasicAuth=false fora de Development. Configure HangfireDashboard:RequireBasicAuth=true.");
            return false;
        }

        // Validar credenciais Basic Auth enviadas pelo browser
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            // Sem credenciais: emitir desafio WWW-Authenticate para o browser abrir o popup de login
            ChallengeBasicAuth(httpContext);
            return false;
        }

        try
        {
            var encodedCredentials = authHeader["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var separatorIndex = decoded.IndexOf(':');
            if (separatorIndex < 0)
            {
                ChallengeBasicAuth(httpContext);
                return false;
            }

            var username = decoded[..separatorIndex];
            var password = decoded[(separatorIndex + 1)..];

            var valid = string.Equals(username, _settings.Username, StringComparison.Ordinal)
                     && string.Equals(password, _settings.Password, StringComparison.Ordinal);

            if (!valid)
            {
                _logger.LogWarning("[Hangfire] Tentativa de acesso ao dashboard com credenciais inválidas. Username: {Username} | IP: {IP}",
                    username,
                    httpContext.Connection.RemoteIpAddress);
                ChallengeBasicAuth(httpContext);
                return false;
            }

            _logger.LogInformation("[Hangfire] Acesso ao dashboard autorizado. Username: {Username} | IP: {IP}",
                username,
                httpContext.Connection.RemoteIpAddress);
            return true;
        }
        catch (FormatException)
        {
            ChallengeBasicAuth(httpContext);
            return false;
        }
    }

    private static void ChallengeBasicAuth(HttpContext httpContext)
    {
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\", charset=\"UTF-8\"";
    }
}
