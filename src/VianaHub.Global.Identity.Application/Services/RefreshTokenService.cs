using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Result;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace VianaHub.Global.Identity.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly INotify _notify;
    private readonly IRefreshTokenDataRepository _refreshRepo;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly ILocalizationService _localization;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenService(
        INotify notify,
        IRefreshTokenDataRepository refreshRepo,
        IRefreshTokenHasher refreshTokenHasher,
        ILocalizationService localization,
        ILogger<RefreshTokenService> logger,
        IOptions<JwtSettings> jwtOptions)
    {
        _notify = notify;
        _refreshRepo = refreshRepo;
        _refreshTokenHasher = refreshTokenHasher;
        _localization = localization;
        _logger = logger;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<RefreshTokenIssueResult> IssueAsync(int tenantId, int appId, int userId, CancellationToken ct)
    {
        var rawToken = GenerateRawToken();
        var tokenHash = _refreshTokenHasher.Hash(rawToken);
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        var entity = new RefreshTokenEntity(tenantId, appId, userId, tokenHash, expiresAt, userId);
        await _refreshRepo.CreateAsync(entity, ct);

        return new RefreshTokenIssueResult
        {
            Token = rawToken,
            ExpiresAt = expiresAt,
            Entity = entity
        };
    }

    public async Task<RefreshTokenRotateResult> RotateAsync(string rawRefreshToken, int tenantId, CancellationToken ct)
    {
        var tokenHash = _refreshTokenHasher.Hash(rawRefreshToken);
        var existing = await _refreshRepo.GetByTokenHashAsync(tokenHash, tenantId, ct);

        if (existing == null || !existing.IsActive())
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Refresh.InvalidRefreshToken"), 401);
            _logger.LogWarning("Refresh token inválido ou expirado para tenant {TenantId}", tenantId);
            return null;
        }

        existing.Revoke(existing.UserId);
        await _refreshRepo.RevokeAsync(existing, ct);

        var newRawToken = GenerateRawToken();
        var newHash = _refreshTokenHasher.Hash(newRawToken);
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        var newEntity = new RefreshTokenEntity(tenantId, existing.AppId, existing.UserId, newHash, expiresAt, existing.UserId);
        await _refreshRepo.CreateAsync(newEntity, ct);

        return new RefreshTokenRotateResult
        {
            OldEntity = existing,
            NewToken = newRawToken,
            NewEntity = newEntity
        };
    }

    private static string GenerateRawToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=');
    }

    public async Task<int> RevokeAllAsync(int userId, int tenantId, int revokedBy, CancellationToken ct)
    {
        var revoked = await _refreshRepo.RevokeAllByUserAsync(userId, tenantId, revokedBy, ct);
        _logger.LogInformation("Revogados {Count} refresh tokens para user {UserId} no tenant {TenantId}", revoked, userId, tenantId);
        return revoked;
    }
}
