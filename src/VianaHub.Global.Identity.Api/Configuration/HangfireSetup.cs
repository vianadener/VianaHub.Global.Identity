using VianaHub.Global.Identity.Api.Filters;
using VianaHub.Global.Identity.Domain.Interfaces;
using Hangfire;
using Hangfire.SqlServer;

namespace VianaHub.Global.Identity.Api.Configuration;

public static class HangfireSetup
{
    public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuração do dashboard Hangfire (Basic Auth)
        services.Configure<HangfireDashboardSettings>(configuration.GetSection("HangfireDashboard"));
        services.AddTransient<HangfireDashboardAuthorizationFilter>();

        var hangfireConn = configuration.GetConnectionString("HangfireConnection");

        services.AddHangfire(configuration =>
        {
            configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                         .UseSimpleAssemblyNameTypeSerializer()
                         .UseRecommendedSerializerSettings()
                         .UseSqlServerStorage(hangfireConn, new SqlServerStorageOptions
                         {
                             SchemaName = "dbo",
                             PrepareSchemaIfNecessary = true,
                             QueuePollInterval = TimeSpan.FromSeconds(15)
                         });
        });

        // O servidor é iniciado no pipeline (UseHangfireServerWithDynamicQueues)
        // para que as filas sejam lidas dinamicamente do banco de dados.

        return services;
    }

    public static IApplicationBuilder UseHangfireServerWithDynamicQueues(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IJobDefinitionDataRepository>();

        var queues = repo.GetAllAsync(CancellationToken.None)
            .GetAwaiter()
            .GetResult()
            .Where(j => !string.IsNullOrWhiteSpace(j.Queue))
            .Select(j => j.Queue!.ToLowerInvariant())
            .Distinct()
            .ToList();

        if (!queues.Contains("default"))
            queues.Add("default");

        app.UseHangfireServer(new BackgroundJobServerOptions
        {
            Queues = [.. queues]
        });

        return app;
    }
}
