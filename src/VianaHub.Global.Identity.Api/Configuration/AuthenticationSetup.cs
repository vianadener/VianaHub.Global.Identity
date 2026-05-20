using VianaHub.Global.Identity.Application.Dto.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VianaHub.Global.Identity.Api.Configuration;

/// <summary>
/// Configuração de autenticação JWT para a API.
/// Implementa configuração segura seguindo as melhores práticas de segurança.
/// </summary>
public static class AuthenticationSetup
{
    /// <summary>
    /// Adiciona e configura autenticação JWT Bearer com suporte a chaves dinâmicas.
    /// </summary>
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Carrega as configurações do JWT do appsettings
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() 
            ?? throw new InvalidOperationException("JwtSettings configuration section is missing in appsettings.json");

        // Obtém a chave mestra para validação de tokens
        var masterKey = configuration["Security:JwtMasterKey"] 
            ?? throw new InvalidOperationException("Security:JwtMasterKey configuration is missing in appsettings.json");

        // Registra as configurações do JWT no container de DI para uso em outras camadas
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Configura autenticação JWT Bearer
        services.AddAuthentication(options =>
        {
            // Define JWT Bearer como esquema padrão para autenticação e desafio
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Configurações de validação do token
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validação do emissor (Issuer)
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidIssuers = jwtSettings.ValidIssuers,

                // Validação da audiência (Audience)
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidAudiences = jwtSettings.ValidAudiences,

                // Validação da chave de assinatura
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(masterKey)),

                // Validação do tempo de expiração
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero, // Remove tolerância de tempo padrão (5 minutos)

                // Exige que o token tenha data de expiração
                RequireExpirationTime = true,
            };

            // Configurações de eventos do JWT Bearer
            options.Events = new JwtBearerEvents
            {
                // Evento disparado quando a autenticação falha
                OnAuthenticationFailed = context =>
                {
                    // Log do erro de autenticação
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Authentication failed: {Message}", context.Exception.Message);

                    // Se o token expirou, adiciona header customizado
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }

                    return Task.CompletedTask;
                },

                // Evento disparado quando o desafio de autenticação é executado (401)
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Challenge triggered for path: {Path}", context.Request.Path);
                    return Task.CompletedTask;
                },

                // Evento disparado quando o token é validado com sucesso
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var userId = context.Principal?.FindFirst("sub")?.Value ?? "Unknown";
                    logger.LogDebug("JWT Token validated successfully for user: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };

            // Configurações de salvamento de tokens
            options.SaveToken = false; // Não salva o token no AuthenticationProperties (segurança)
            options.RequireHttpsMetadata = false; // Permite HTTP em desenvolvimento (configurar por ambiente em produção)
        });

        // Configura políticas de autorização
        services.AddAuthorization(options =>
        {
            // Política padrão: usuário autenticado
            options.FallbackPolicy = null; // Permite endpoints sem [Authorize] serem públicos

            // Políticas baseadas em roles
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("BackOfficeOnly", policy => policy.RequireRole("BackOffice"));
            options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
            options.AddPolicy("OperatorOnly", policy => policy.RequireRole("Operator"));

            // Política que permite múltiplas roles
            options.AddPolicy("AdminOrBackOffice", policy => 
                policy.RequireRole("Admin", "BackOffice"));

            options.AddPolicy("StaffOnly", policy => 
                policy.RequireRole("Admin", "BackOffice", "Manager", "Operator"));
        });

        return services;
    }
}
