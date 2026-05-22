using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Tools.Cryptography;
using VianaHub.Global.Middleware.Lib.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace VianaHub.Global.Identity.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly INotify _notify;
    private readonly IUserRoleDataRepository _userRoleRepo;
    private readonly IRolePermissionDataRepository _rolePermissionRepo;
    private readonly ITenantDataRepository _tenantRepo;
    private readonly IJwtKeyDataRepository _jwtKeyRepo;
    private readonly ISecretProvider _secretProvider;
    private readonly ILocalizationService _localization;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(
        INotify notify,
        IUserRoleDataRepository userRoleRepo,
        IRolePermissionDataRepository rolePermissionRepo,
        ITenantDataRepository tenantRepo,
        IJwtKeyDataRepository jwtKeyRepo,
        ISecretProvider secretProvider,
        ILocalizationService localization,
        ILogger<JwtTokenService> logger,
        IOptions<JwtSettings> jwtOptions)
    {
        _notify = notify;
        _userRoleRepo = userRoleRepo;
        _rolePermissionRepo = rolePermissionRepo;
        _tenantRepo = tenantRepo;
        _jwtKeyRepo = jwtKeyRepo;
        _secretProvider = secretProvider;
        _localization = localization;
        _logger = logger;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(UserEntity user, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tenantId", user.TenantId.ToString()),
            new("appId", user.UserRoles?.FirstOrDefault()?.AppId.ToString() ?? "0"),
            new(JwtRegisteredClaimNames.Name, user.Name ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        string permissionsJson = null;

        try
        {
            var userRoles = await _userRoleRepo.GetByUserIdAsync(user.Id, ct);

            var roleNames = userRoles?
                .Where(r => r?.Role != null)
                .Select(r => r.Role.Name?.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.ToLower())
                .Distinct()
                .ToList() ?? [];

            foreach (var roleName in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                claims.Add(new Claim("role", roleName));
            }

            var permissionsByResource = new Dictionary<string, HashSet<string>>();

            foreach (var userRole in userRoles ?? Enumerable.Empty<UserRoleEntity>())
            {
                if (userRole?.Role is null) continue;
                var rolePerms = await _rolePermissionRepo.GetByRoleAsync(userRole.Role.Id, user.TenantId, ct);
                if (rolePerms is null || !rolePerms.Any()) continue;

                foreach (var rp in rolePerms)
                {
                    var resource = rp.Resource?.Name?.Trim();
                    var action = rp.Action?.Name?.Trim();
                    if (string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(action)) continue;

                    var resourceKey = resource.ToLower();
                    var actionKey = action.ToLower();

                    if (!permissionsByResource.TryGetValue(resourceKey, out var actions))
                    {
                        actions = new HashSet<string>();
                        permissionsByResource[resourceKey] = actions;
                    }

                    actions.Add(actionKey);
                }
            }

            if (permissionsByResource.Count > 0)
            {
                var serializable = permissionsByResource.ToDictionary(k => k.Key, v => v.Value.OrderBy(x => x).ToList());
                permissionsJson = JsonSerializer.Serialize(serializable);
                _logger.LogDebug("Permissions incluídas no token para user {UserId}: {ResourceCount} recursos", user.Id, serializable.Count);
            }
            else
            {
                _logger.LogWarning("Nenhuma permissão encontrada para user {UserId}", user.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao incluir roles/permissions no token para user {UserId}", user.Id);
        }

        var keyEntity = await _jwtKeyRepo.GetActiveKeyAsync(user.TenantId, ct);
        if (keyEntity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.NoActiveKey"), 500);
            _logger.LogError("Nenhuma chave JWT ativa encontrada para tenant {TenantId}", user.TenantId);
            return (null, DateTime.MinValue);
        }

        var masterKey = _secretProvider.GetMasterKey();
        if (string.IsNullOrWhiteSpace(masterKey))
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.EncryptionKeyMissing"), 500);
            _logger.LogError("Master key for JWT decryption not available");
            return (null, DateTime.MinValue);
        }

        string privatePem;
        try
        {
            privatePem = CryptoRSA.DecryptPrivateKey(keyEntity.PrivateKeyEncrypted, masterKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao descriptografar chave privada para tenant {TenantId}", user.TenantId);
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.InvalidPrivateKey"), 500);
            return (null, DateTime.MinValue);
        }

        try
        {
            var rsa = RSA.Create();
            try
            {
                var privateKeyBytes = CryptoRSA.ExtractBytesFromPem(privatePem, "PRIVATE KEY");
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);

                var rsaKey = new RsaSecurityKey(rsa) { KeyId = keyEntity.KeyId.ToString() };
                rsaKey.CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false };

                var creds = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: expires,
                    signingCredentials: creds
                );

                if (!string.IsNullOrWhiteSpace(permissionsJson))
                {
                    try
                    {
                        var permissionsDict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(permissionsJson);
                        token.Payload["permissions"] = permissionsDict;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Falha ao parsear permissionsJson antes de inserir no payload");
                    }
                }

                var handler = new JwtSecurityTokenHandler();
                var tokenString = handler.WriteToken(token);
                return (tokenString, expires);
            }
            finally
            {
                try { rsa.Dispose(); } catch { }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar token JWT para tenant {TenantId}", user.TenantId);
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Token.CreateFailed"), 500);
            return (null, DateTime.MinValue);
        }
    }
}
