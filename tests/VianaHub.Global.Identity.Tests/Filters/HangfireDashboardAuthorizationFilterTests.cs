using VianaHub.Global.Identity.Api.Configuration;
using VianaHub.Global.Identity.Api.Filters;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Dashboard;
using Hangfire.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace VianaHub.Global.Identity.Tests.Filters;

public class HangfireDashboardAuthorizationFilterTests
{
    private readonly Mock<ILogger<HangfireDashboardAuthorizationFilter>> _loggerMock;

    public HangfireDashboardAuthorizationFilterTests()
    {
        _loggerMock = new Mock<ILogger<HangfireDashboardAuthorizationFilter>>();
    }

    private HangfireDashboardAuthorizationFilter CriarFilter(HangfireDashboardSettings settings)
    {
        var options = Options.Create(settings);
        return new HangfireDashboardAuthorizationFilter(options, _loggerMock.Object);
    }

    private static DashboardContext CriarDashboardContext(
        HttpContext httpContext,
        IWebHostEnvironment? env = null)
    {
        var servicesMock = new Mock<IServiceProvider>();
        if (env != null)
            servicesMock.Setup(x => x.GetService(typeof(IWebHostEnvironment))).Returns(env);

        httpContext.RequestServices = servicesMock.Object;

        return new AspNetCoreDashboardContext(new Mock<JobStorage>().Object, new DashboardOptions(), httpContext);
    }

    private static IWebHostEnvironment CriarEnvironment(bool isDevelopment)
    {
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(x => x.EnvironmentName).Returns(isDevelopment ? "Development" : "Production");
        envMock.Setup(x => x.ContentRootPath).Returns("/");
        envMock.Setup(x => x.WebRootPath).Returns("/");
        return envMock.Object;
    }

    private static string GerarBasicAuthHeader(string username, string password)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return $"Basic {credentials}";
    }

    [Fact(DisplayName = "HangfireDashboardAuthorizationFilter - Deve bloquear quando RequireBasicAuth=false fora de Development")]
    [Trait("Api", "")]
    public void Authorize_RequireBasicAuthFalse_ForaDeDesenvolvimento_DeveBloquear()
    {
        var settings = new HangfireDashboardSettings { RequireBasicAuth = false };
        var filter = CriarFilter(settings);

        var httpContext = new DefaultHttpContext();
        var env = CriarEnvironment(isDevelopment: false);
        var context = CriarDashboardContext(httpContext, env);

        var resultado = filter.Authorize(context);

        Assert.False(resultado);
    }

    [Fact(DisplayName = "HangfireDashboardAuthorizationFilter - Deve bloquear quando sem header de autorização")]
    [Trait("Api", "")]
    public void Authorize_SemHeaderDeAutorizacao_DeveBloquear()
    {
        var settings = new HangfireDashboardSettings { RequireBasicAuth = true, Username = "admin", Password = "pass" };
        var filter = CriarFilter(settings);

        var httpContext = new DefaultHttpContext();
        var env = CriarEnvironment(isDevelopment: false);
        var context = CriarDashboardContext(httpContext, env);

        var resultado = filter.Authorize(context);

        Assert.False(resultado);
    }

    [Fact(DisplayName = "HangfireDashboardAuthorizationFilter - Deve bloquear quando credenciais inválidas")]
    [Trait("Api", "")]
    public void Authorize_CredenciaisInvalidas_DeveBloquear()
    {
        var settings = new HangfireDashboardSettings { RequireBasicAuth = true, Username = "admin", Password = "senha-correta" };
        var filter = CriarFilter(settings);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = GerarBasicAuthHeader("admin", "senha-errada");
        var env = CriarEnvironment(isDevelopment: false);
        var context = CriarDashboardContext(httpContext, env);

        var resultado = filter.Authorize(context);

        Assert.False(resultado);
    }

    [Fact(DisplayName = "HangfireDashboardAuthorizationFilter - Deve autorizar quando credenciais corretas")]
    [Trait("Api", "")]
    public void Authorize_CredenciaisCorretas_DeveAutorizar()
    {
        var settings = new HangfireDashboardSettings { RequireBasicAuth = true, Username = "admin", Password = "senha123" };
        var filter = CriarFilter(settings);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = GerarBasicAuthHeader("admin", "senha123");
        var env = CriarEnvironment(isDevelopment: false);
        var context = CriarDashboardContext(httpContext, env);

        var resultado = filter.Authorize(context);

        Assert.True(resultado);
    }

    [Fact(DisplayName = "HangfireDashboardAuthorizationFilter - Deve bloquear quando header Basic Auth malformado (sem separador ':')")]
    [Trait("Api", "")]
    public void Authorize_HeaderMalformadoSemSeparador_DeveBloquear()
    {
        var settings = new HangfireDashboardSettings { RequireBasicAuth = true, Username = "admin", Password = "pass" };
        var filter = CriarFilter(settings);

        var httpContext = new DefaultHttpContext();
        var semSeparador = Convert.ToBase64String(Encoding.UTF8.GetBytes("adminpasswordsemseparador"));
        httpContext.Request.Headers["Authorization"] = $"Basic {semSeparador}";
        var env = CriarEnvironment(isDevelopment: false);
        var context = CriarDashboardContext(httpContext, env);

        var resultado = filter.Authorize(context);

        Assert.False(resultado);
    }

    [Fact(DisplayName = "HangfireDashboardAuthorizationFilter - Deve bloquear quando header Base64 inválido")]
    [Trait("Api", "")]
    public void Authorize_HeaderBase64Invalido_DeveBloquear()
    {
        var settings = new HangfireDashboardSettings { RequireBasicAuth = true, Username = "admin", Password = "pass" };
        var filter = CriarFilter(settings);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Basic nao-e-base64-valido!!!";
        var env = CriarEnvironment(isDevelopment: false);
        var context = CriarDashboardContext(httpContext, env);

        var resultado = filter.Authorize(context);

        Assert.False(resultado);
    }

    [Fact(DisplayName = "HangfireDashboardAuthorizationFilter - Deve bloquear quando header não começa com 'Basic '")]
    [Trait("Api", "")]
    public void Authorize_HeaderSemPrefixoBasic_DeveBloquear()
    {
        var settings = new HangfireDashboardSettings { RequireBasicAuth = true, Username = "admin", Password = "pass" };
        var filter = CriarFilter(settings);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Bearer token-jwt-qualquer";
        var env = CriarEnvironment(isDevelopment: false);
        var context = CriarDashboardContext(httpContext, env);

        var resultado = filter.Authorize(context);

        Assert.False(resultado);
    }
}
