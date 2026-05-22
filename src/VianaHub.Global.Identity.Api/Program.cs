using VianaHub.Global.Middleware.Lib.Middleware;
using VianaHub.Global.Identity.Api.Configuration;
using VianaHub.Global.Identity.Api.Configuration.Swagger;
using VianaHub.Global.Identity.Api.Filters;
using VianaHub.Global.Identity.Api.i18n;
using VianaHub.Global.Identity.Api.Middleware;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Data.Context;
using VianaHub.Global.Identity.Infra.Data.Interceptors;
using VianaHub.Global.Identity.Infra.Data.Tools;
using VianaHub.Global.Identity.Infra.IoC;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilog();

ConfigurationValidator.ValidateConfiguration(builder.Configuration, builder.Environment);

// Serviços de infraestrutura da API
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger(builder.Configuration, builder.Environment);
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ActionMappingProfile).Assembly));
builder.Services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    var tenantConnectionInterceptor = serviceProvider.GetRequiredService<TenantSessionConnectionInterceptor>();
    var tenantCommandInterceptor = serviceProvider.GetRequiredService<TenantSessionCommandInterceptor>();
    var telemetryInterceptor = serviceProvider.GetRequiredService<TelemetryInterceptor>();

    options.AddInterceptors(
        tenantConnectionInterceptor,
        tenantCommandInterceptor,
        telemetryInterceptor
    );
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddJwt(builder.Configuration);
builder.Services.AddRouteValidatorSetup();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BackOffice", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});
builder.Services.AddRateLimitingConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddHangfireConfiguration(builder.Configuration);
builder.Services.AddHealthCheckConfiguration(builder.Configuration);
builder.Services.AddDependencyInjection(builder.Configuration);

builder.Services.AddScoped<ICurrentUserService, CurrentUserApiService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

var app = builder.Build();

var autoMigrate = builder.Configuration.GetValue<bool>("Database:AutoMigrate", true);

await app.InitializeDatabaseAsync(autoMigrate);

// Servir arquivos estáticos (necessário para custom.js e custom.css do Swagger)
app.UseStaticFiles();

// Configure the HTTP request pipeline.
var swaggerEnabled = builder.Configuration.GetValue<bool>("Swagger:Enabled", !app.Environment.IsProduction());
if (swaggerEnabled && !app.Environment.IsProduction())
{
    app.UseSwaggerConfiguration(app.Environment);
}

// Middlewares customizados
app.UseMiddleware<RequestLocalizationMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<HttpProtocolExceptionMiddleware>();
app.UseMiddleware<JsonExceptionMiddleware>();
app.UseMiddleware<DomainExceptionMiddleware>();
app.UseMiddleware<NotificationMiddleware>();

app.UseHttpsRedirection();

var hangfireEnabled = builder.Configuration.GetValue<bool>("HangfireDashboard:Enabled", false);
if (hangfireEnabled && !app.Environment.IsProduction())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { app.Services.GetRequiredService<HangfireDashboardAuthorizationFilter>() }
    });
}

app.UseHangfireServerWithDynamicQueues();

var rateLimitingEnabled = builder.Configuration.GetValue<bool>("RateLimiting:EnableRateLimiting", true);
if (rateLimitingEnabled)
{
    app.UseRateLimiter();
}

var corsEnabled = builder.Configuration.GetValue<bool>("Cors:EnableCors", true);
if (corsEnabled)
{
    var policyName = builder.Configuration.GetValue<string>("Cors:PolicyName") ?? "GeritCorsPolicy";
    app.UseCors(policyName);
}

// Middlewares de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.UseHealthCheckEndpoint();

app.MapEndpointsFromAssembly();

app.Run();

public partial class Program { }
