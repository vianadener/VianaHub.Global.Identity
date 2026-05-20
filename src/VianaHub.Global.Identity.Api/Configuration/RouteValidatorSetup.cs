using VianaHub.Global.Identity.Api.Validations.Action;
using VianaHub.Global.Identity.Api.Validations.Auth;
using VianaHub.Global.Identity.Api.Validations.Resource;
using VianaHub.Global.Identity.Api.Validations.Role;
using VianaHub.Global.Identity.Api.Validations.Tenant;
using VianaHub.Global.Identity.Api.Validations.User;
using VianaHub.Global.Identity.Application.Dto.Request.Action;
using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Dto.Request.Resource;
using VianaHub.Global.Identity.Application.Dto.Request.Role;
using VianaHub.Global.Identity.Application.Dto.Request.Tenant;
using VianaHub.Global.Identity.Application.Dto.Request.User;
using FluentValidation;

namespace VianaHub.Global.Identity.Api.Configuration;

/// <summary>
/// Classe responsável por registrar os validadores de rota na injeção de dependência.
/// </summary>
public static class RouteValidatorSetup
{
    /// <summary>
    /// Adiciona os validadores de rota à coleção de serviços.
    /// </summary>
    /// <param name="services">A coleção de serviços da aplicação.</param>
    /// <returns>A própria coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddRouteValidatorSetup(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Action Route Validators
        services.AddScoped<IValidator<CreateActionRequest>, CreateActionRouteValidator>();
        services.AddScoped<IValidator<UpdateActionRequest>, UpdateActionRouteValidator>();

        // Resource Route Validators
        services.AddScoped<IValidator<CreateResourceRequest>, CreateResourceRouteValidator>();
        services.AddScoped<IValidator<UpdateResourceRequest>, UpdateResourceRouteValidator>();

        // Role Route Validators
        services.AddScoped<IValidator<CreateRoleRequest>, CreateRoleRouteValidator>();
        services.AddScoped<IValidator<UpdateRoleRequest>, UpdateRoleRouteValidator>();

        // Tenant Route Validators
        services.AddScoped<IValidator<CreateTenantRequest>, CreateTenantRouteValidator>();
        services.AddScoped<IValidator<UpdateTenantRequest>, UpdateTenantRouteValidator>();

        // User Route Validators
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRouteValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRouteValidator>();
        services.AddScoped<IValidator<UpdatePasswordRequest>, UpdatePasswordRouteValidator>();

        // Auth Route Validators
        services.AddScoped<IValidator<RegisterRequest>, RegisterRouteValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRouteValidator>();
        services.AddScoped<IValidator<RefreshRequest>, RefreshRouteValidator>();
        services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRouteValidator>();
        services.AddScoped<IValidator<ValidateResetTokenRequest>, ValidateResetTokenRouteValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRouteValidator>();


        return services;
    }
}
