using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Services;
using VianaHub.Global.Identity.Infra.Integration.Email;
using VianaHub.Global.Identity.Domain.Validators.Action;
using VianaHub.Global.Identity.Domain.Validators.App;
using VianaHub.Global.Identity.Domain.Validators.Job;
using VianaHub.Global.Identity.Domain.Validators.Jwt;
using VianaHub.Global.Identity.Domain.Validators.Resource;
using VianaHub.Global.Identity.Domain.Validators.Role;
using VianaHub.Global.Identity.Domain.Validators.RolePermission;
using VianaHub.Global.Identity.Domain.Validators.Tenant;
using VianaHub.Global.Identity.Domain.Validators.User;
using VianaHub.Global.Identity.Domain.Validators.UserRole;
using VianaHub.Global.Identity.Infra.Data.Context;
using VianaHub.Global.Identity.Infra.Data.Interceptors;
using VianaHub.Global.Identity.Infra.Data.Providers;
using VianaHub.Global.Identity.Infra.Data.Repository;
using VianaHub.Global.Identity.Infra.Job.HostedServices;
using VianaHub.Global.Identity.Infra.Job.Interfaces;
using VianaHub.Global.Identity.Infra.Job.Jobs.Maintenance;
using VianaHub.Global.Identity.Infra.Job.Jobs.Security;
using VianaHub.Global.Identity.Infra.Job.Services;
using VianaHub.Global.Middleware.Lib.Notifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VianaHub.Global.Identity.Infra.IoC;

/// <summary>
/// Configuração centralizada de injeção de dependências para todas as camadas do Identity.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra todos os serviços da aplicação no container de DI.
    /// </summary>
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // Notificações (Scoped para manter estado durante a requisição)
        services.AddScoped<INotify, Notify>();

        // Context de Tenant para Request (Scoped)
        services.AddScoped<IRequestTenantContext, RequestTenantContext>();

        // Interceptors do Entity Framework (Scoped)
        services.AddScoped<TenantSessionConnectionInterceptor>();
        services.AddScoped<TenantSessionCommandInterceptor>();
        services.AddScoped<TelemetryInterceptor>();

        // Serviços de Infraestrutura Base
        services.AddScoped<ISecretProvider, EnvironmentSecretProvider>();
        services.AddScoped<IFileValidationService, FileValidationService>();

        // Validators (Scoped)
        services.AddScoped<IValidator<UserRoleEntity>, UserRoleValidator>();
        services.AddScoped<IValidator<RolePermissionEntity>, RolePermissionValidator>();

        services.AddScoped<IEntityDomainValidator<ActionEntity>, ActionValidator>();
        services.AddScoped<IEntityDomainValidator<AppEntity>, AppValidator>();
        services.AddScoped<IEntityDomainValidator<ResourceEntity>, ResourceValidator>();
        services.AddScoped<IEntityDomainValidator<RoleEntity>, RoleValidator>();
        services.AddScoped<IEntityDomainValidator<TenantEntity>, TenantValidator>();
        services.AddScoped<IEntityDomainValidator<UserEntity>, UserValidator>();
        services.AddScoped<IEntityDomainValidator<JobDefinitionEntity>, JobDefinitionValidator>();
        services.AddScoped<IEntityDomainValidator<JwtKeyEntity>, JwtKeyValidator>();
        services.AddScoped<IEntityDomainValidator<UserRoleEntity>, UserRoleValidator>();

        // Application - Common Services
        // Application - App Services
        services.AddScoped<IActionAppService, ActionAppService>();
        services.AddScoped<IAppAppService, AppAppService>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IForgotPasswordAppService, ForgotPasswordAppService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IJwtKeyAppService, JwtKeyAppService>();
        services.AddScoped<IJobAppService, JobAppService>();
        services.AddScoped<IResourceAppService, ResourceAppService>();
        services.AddScoped<IRoleAppService, RoleAppService>();
        services.AddScoped<IRolePermissionAppService, RolePermissionAppService>();
        services.AddScoped<ITenantAppService, TenantAppService>();
        services.AddScoped<IUserAppService, UserAppService>();
        services.AddScoped<IUserRoleAppService, UserRoleAppService>();

        // Domain
        services.AddScoped<IUserRoleDomainService, UserRoleDomainService>();
        services.AddScoped<IActionDomainService, ActionDomainService>();
        services.AddScoped<IAppDomainService, AppDomainService>();
        services.AddScoped<IResourceDomainService, ResourceDomainService>();
        services.AddScoped<IRoleDomainService, RoleDomainService>();
        services.AddScoped<ITenantDomainService, TenantDomainService>();
        services.AddScoped<IUserDomainService, UserDomainService>();
        services.AddScoped<IRolePermissionDomainService, RolePermissionDomainService>();
        services.AddScoped<IJwtKeyDomainService, JwtKeyDomainService>();
        services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();

        // Infra.Data - Repositories
        services.AddScoped<IActionDataRepository, ActionDataRepository>();
        services.AddScoped<IAppDataRepository, AppDataRepository>();
        services.AddScoped<IResourceDataRepository, ResourceDataRepository>();
        services.AddScoped<IRoleDataRepository, RoleDataRepository>();
        services.AddScoped<ITenantDataRepository, TenantDataRepository>();
        services.AddScoped<IJwtKeyDataRepository, JwtKeyDataRepository>();
        services.AddScoped<IUserDataRepository, UserDataRepository>();
        services.AddScoped<IUserRoleDataRepository, UserRoleDataRepository>();
        services.AddScoped<IRolePermissionDataRepository, RolePermissionDataRepository>();
        services.AddScoped<IRefreshTokenDataRepository, RefreshTokenDataRepository>();
        services.AddScoped<IPasswordResetTokenDataRepository, PasswordResetTokenDataRepository>();
        services.AddScoped<IJobDefinitionDataRepository, JobDefinitionDataRepository>();

        // Infra.Messaging (Email sender no-op por enquanto)
        services.AddScoped<IEmailSender, NoOpEmailSender>();

        // Hangfire Job service
        services.AddScoped<IJobSchedulerService, HangfireJobService>();
        services.AddScoped<IJobExecutor, HangfireJobExecutor>();
        services.AddScoped<IJobSyncService, JobSyncService>();
        services.AddScoped<ScheduledSyncJobDefinitionsJob>();
        services.AddScoped<JwtKeyRotationJob>();
        services.AddHostedService<JobSyncHostedService>();

        // Data Context - Entity Framework Core
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString));


        return services;
    }
}
