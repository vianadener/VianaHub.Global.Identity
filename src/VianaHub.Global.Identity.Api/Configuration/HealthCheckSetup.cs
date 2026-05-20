using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace VianaHub.Global.Identity.Api.Configuration;

/// <summary>
/// Provides extension methods for configuring health check endpoints in an ASP.NET Core application.
/// </summary>
/// <remarks>This class contains static methods to simplify the setup of health check endpoints using the
/// application's request pipeline. It is intended to be used as part of the application's startup
/// configuration.</remarks>
public static class HealthCheckSetup
{
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            hcBuilder.AddSqlServer(
                connectionString: connectionString,
                healthQuery: "SELECT 1",
                configure: null,
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "sqlserver"]);

            hcBuilder.AddDbContextCheck<IdentityDbContext>(
                name: "dbcontext",
                failureStatus: HealthStatus.Degraded,
                tags: ["db", "efcore"]);
        }

        return services;
    }

    public static IApplicationBuilder UseHealthCheckEndpoint(this IApplicationBuilder app)
    {
        var routeBuilder = (IEndpointRouteBuilder)app;

        routeBuilder.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    details = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                    }),
                });
                await context.Response.WriteAsync(result);
            },
        });

        return app;
    }
}
