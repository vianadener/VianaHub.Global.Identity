using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace VianaHub.Global.Identity.Api.Configuration;

/// <summary>
/// Provides extension methods for configuring rate limiting services using application configuration settings.
/// </summary>
/// <remarks>This class is intended to be used during application startup to register and configure rate limiting
/// policies based on values specified in the application's configuration. It enables consistent setup of rate limiting
/// across the application by centralizing configuration logic.</remarks>
public static class RateLimitingSetup
{
    /// <summary>
    /// Configures fixed window rate limiting for the application using settings from the specified configuration.
    /// </summary>
    /// <remarks>The method reads rate limiting options such as permit limit, window duration, queue limit,
    /// and queue processing order from the 'RateLimiting' section of the configuration. If 'QueueProcessingOrder' is
    /// not specified, 'OldestFirst' is used by default.</remarks>
    /// <param name="services">The service collection to which the rate limiting configuration will be added.</param>
    /// <param name="configuration">The application configuration containing the 'RateLimiting' section with rate limiting settings.</param>
    /// <returns>The same service collection instance with rate limiting services configured.</returns>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitSection = configuration.GetSection("RateLimiting");
        int permitLimit = rateLimitSection.GetValue<int>("PermitLimit");
        int windowMinutes = rateLimitSection.GetValue<int>("WindowMinutes");
        int queueLimit = rateLimitSection.GetValue<int>("QueueLimit");
        string queueProcessingOrder = rateLimitSection.GetValue<string>("QueueProcessingOrder") ?? "OldestFirst";

        // Authentication endpoints policy (more restrictive)
        var authSection = rateLimitSection.GetSection("AuthenticationEndpoints");
        int authPermitLimit = authSection.GetValue<int>("PermitLimit");
        int authWindowMinutes = authSection.GetValue<int>("WindowMinutes");
        int authQueueLimit = authSection.GetValue<int>("QueueLimit");

        // Refresh token endpoints policy (less restrictive)
        var refreshSection = rateLimitSection.GetSection("RefreshTokenEndpoints");
        int refreshPermitLimit = refreshSection.GetValue<int>("PermitLimit");
        int refreshWindowMinutes = refreshSection.GetValue<int>("WindowMinutes");
        int refreshQueueLimit = refreshSection.GetValue<int>("QueueLimit");

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("default", limiterOptions =>
            {
                limiterOptions.PermitLimit = permitLimit;
                limiterOptions.Window = TimeSpan.FromMinutes(windowMinutes);
                limiterOptions.QueueLimit = queueLimit;
                limiterOptions.QueueProcessingOrder = queueProcessingOrder.ToLower() switch
                {
                    "newestfirst" => QueueProcessingOrder.NewestFirst,
                    _ => QueueProcessingOrder.OldestFirst
                };
            });

            // Authentication policy - more restrictive for login, register, forgot-password, reset-password
            options.AddFixedWindowLimiter("authentication", limiterOptions =>
            {
                limiterOptions.PermitLimit = authPermitLimit > 0 ? authPermitLimit : 100;
                limiterOptions.Window = TimeSpan.FromMinutes(authWindowMinutes > 0 ? authWindowMinutes : 1);
                limiterOptions.QueueLimit = authQueueLimit > 0 ? authQueueLimit : 20;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Refresh token policy - less restrictive for token renewal
            options.AddFixedWindowLimiter("refreshtoken", limiterOptions =>
            {
                limiterOptions.PermitLimit = refreshPermitLimit > 0 ? refreshPermitLimit : 200;
                limiterOptions.Window = TimeSpan.FromMinutes(refreshWindowMinutes > 0 ? refreshWindowMinutes : 1);
                limiterOptions.QueueLimit = refreshQueueLimit > 0 ? refreshQueueLimit : 50;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }
}
