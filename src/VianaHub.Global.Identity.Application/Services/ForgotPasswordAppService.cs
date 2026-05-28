using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Dto.Response.Auth;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Middleware.Lib.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace VianaHub.Global.Identity.Application.Services;

public class ForgotPasswordAppService : IForgotPasswordAppService
{
    private const int ResetTokenTtlMinutes = 15;
    private const int RateLimitMaxAttempts = 5;
    private const int RateLimitWindowMinutes = 60;

    private readonly INotify _notify;
    private readonly IUserDataRepository _userRepo;
    private readonly ITenantDataRepository _tenantRepo;
    private readonly IPasswordResetTokenDataRepository _resetTokenRepo;
    private readonly IRefreshTokenDataRepository _refreshTokenRepo;
    private readonly IEmailSender _emailSender;
    private readonly ILocalizationService _localization;
    private readonly IRequestTenantContext _requestTenantContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordAppService> _logger;

    public ForgotPasswordAppService(
        INotify notify,
        IUserDataRepository userRepo,
        ITenantDataRepository tenantRepo,
        IPasswordResetTokenDataRepository resetTokenRepo,
        IRefreshTokenDataRepository refreshTokenRepo,
        IEmailSender emailSender,
        ILocalizationService localization,
        IRequestTenantContext requestTenantContext,
        IConfiguration configuration,
        ILogger<ForgotPasswordAppService> logger)
    {
        _notify = notify;
        _userRepo = userRepo;
        _tenantRepo = tenantRepo;
        _resetTokenRepo = resetTokenRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _emailSender = emailSender;
        _localization = localization;
        _requestTenantContext = requestTenantContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var genericResponse = new ForgotPasswordResponse(
            Message: _localization.GetMessage("Application.Service.Auth.ForgotPassword.GenericSuccess")
        );

        // Resolve o tenant a partir do LoginIdentifier — sem expor TenantId ao cliente
        var tenant = await _tenantRepo.GetByLoginIdentifierAsync(request.LoginIdentifier, ct);
        if (tenant is null)
        {
            _logger.LogInformation("ForgotPassword: nenhum tenant encontrado para loginIdentifier={LoginIdentifier}", request.LoginIdentifier);
            return genericResponse;
        }

        _requestTenantContext.SetTenantId(tenant.Id);

        var normalizedLogin = request.LoginIdentifier.Trim().ToUpperInvariant();

        var user = await _userRepo.GetByNormalizedLoginAsync(tenant.Id, normalizedLogin, ct);
        if (user is null || !user.IsActive)
        {
            _logger.LogInformation("ForgotPassword: usuário não encontrado ou inativo para tenantId={TenantId}", tenant.Id);
            return genericResponse;
        }

        // Rate limit: máx 5 tentativas por usuário nos últimos 60 minutos
        var since = DateTime.UtcNow.AddMinutes(-RateLimitWindowMinutes);
        var recentAttempts = await _resetTokenRepo.CountRecentByUserAsync(user.Id, tenant.Id, since, ct);
        if (recentAttempts >= RateLimitMaxAttempts)
        {
            _logger.LogWarning("ForgotPassword: rate limit atingido para userId={UserId}, tenantId={TenantId}", user.Id, tenant.Id);
            return genericResponse;
        }

        // Gera token UUID → hash SHA-256
        var rawToken = Guid.NewGuid().ToString("N");
        var tokenHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));

        var expiresAt = DateTime.UtcNow.AddMinutes(ResetTokenTtlMinutes);
        var entity = new PasswordResetTokenEntity(tenant.Id, user.Id, tokenHash, expiresAt, 0);

        var created = await _resetTokenRepo.CreateAsync(entity, ct);
        if (!created)
        {
            _logger.LogError("ForgotPassword: falha ao persistir token de reset para userId={UserId}", user.Id);
            return genericResponse;
        }

        var baseUrl = _configuration["App:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
        var resetLink = $"{baseUrl}/reset?token={rawToken}";

        try
        {
            await _emailSender.SendPasswordResetAsync(user.LoginIdentifier!, user.Name!, resetLink, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForgotPassword: falha ao enviar email para userId={UserId}", user.Id);
        }

        return genericResponse;
    }

    public async Task<ValidateResetTokenResponse> ValidateResetTokenAsync(ValidateResetTokenRequest request, CancellationToken ct)
    {
        var tokenHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Token.Trim()));

        var entity = await _resetTokenRepo.GetByTokenHashAsync(tokenHash, ct);
        if (entity is null || !entity.IsValid())
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.ValidateResetToken.InvalidOrExpired"), 400);
            return new ValidateResetTokenResponse(IsValid: false);
        }

        _requestTenantContext.SetTenantId(entity.TenantId);

        var user = await _userRepo.GetByIdAsync(entity.TenantId, entity.UserId, ct);
        if (user is null || !user.IsActive)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.ValidateResetToken.InvalidOrExpired"), 400);
            return new ValidateResetTokenResponse(IsValid: false);
        }

        return new ValidateResetTokenResponse(IsValid: true);
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, string ipAddress, string userAgent, CancellationToken ct)
    {
        var tokenHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Token.Trim()));

        var tokenEntity = await _resetTokenRepo.GetByTokenHashAsync(tokenHash, ct);

        if (tokenEntity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.ResetPassword.InvalidToken"), 409);
            return new ResetPasswordResponse(Message: string.Empty);
        }

        if (tokenEntity.Used)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.ResetPassword.TokenAlreadyUsed"), 410);
            return new ResetPasswordResponse(Message: string.Empty);
        }

        if (DateTime.UtcNow >= tokenEntity.ExpiresAt)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.ResetPassword.TokenExpired"), 410);
            return new ResetPasswordResponse(Message: string.Empty);
        }

        _requestTenantContext.SetTenantId(tokenEntity.TenantId);

        var user = await _userRepo.GetByIdAsync(tokenEntity.TenantId, tokenEntity.UserId, ct);
        if (user is null || !user.IsActive)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.ResetPassword.InvalidToken"), 409);
            return new ResetPasswordResponse(Message: string.Empty);
        }

        var newPasswordHash = DomainExtensions.HashClientSecret(request.NewPassword);
        user.UpdatePassword(newPasswordHash, user.Id);

        var userUpdated = await _userRepo.UpdateAsync(user, ct);
        if (!userUpdated)
        {
            _logger.LogError("ResetPassword: falha ao atualizar senha do userId={UserId}", user.Id);
            _notify.Add(_localization.GetMessage("Application.Service.Auth.ResetPassword.UpdateFailed"), 500);
            return new ResetPasswordResponse(Message: string.Empty);
        }

        tokenEntity.MarkAsUsed(user.Id);
        await _resetTokenRepo.UpdateAsync(tokenEntity, ct);

        await _refreshTokenRepo.RevokeAllByUserAsync(user.Id, tokenEntity.TenantId, user.Id, ct);

        _logger.LogInformation(
            "ResetPassword: senha alterada com sucesso — userId={UserId}, tenantId={TenantId}, ip={IpAddress}, userAgent={UserAgent}",
            user.Id, tokenEntity.TenantId, ipAddress, userAgent);

        try
        {
            await _emailSender.SendPasswordResetConfirmationAsync(user.LoginIdentifier!, user.Name!, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetPassword: falha ao enviar email de confirmação para userId={UserId}", user.Id);
        }

        return new ResetPasswordResponse(
            Message: _localization.GetMessage("Application.Service.Auth.ResetPassword.Success")
        );
    }
}
