using AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Dto.Response.Auth;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VianaHub.Global.Middleware.Lib.Notifications;

namespace VianaHub.Global.Identity.Application.Services;

public class AuthAppService : IAuthAppService
{
    private readonly INotify _notify;
    private readonly IMapper _mapper;
    private readonly IUserDataRepository _userRepo;
    private readonly ITenantDataRepository _tenantRepo;
    private readonly ILocalizationService _localization;
    private readonly ICurrentUserService _currentUser;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthAppService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IdentityDbContext _dbContext;
    private readonly IRequestTenantContext _requestTenantContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private int TenantId { get; set; }

    public AuthAppService(
        INotify notify,
        IMapper mapper,
        IUserDataRepository userRepo,
        ITenantDataRepository tenantRepo,
        ILocalizationService localization,
        ICurrentUserService currentUser,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthAppService> logger,
        IConfiguration configuration,
        IdentityDbContext dbContext,
        IRequestTenantContext requestTenantContext,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService)
    {
        _notify = notify;
        _mapper = mapper;
        _userRepo = userRepo;
        _tenantRepo = tenantRepo;
        _localization = localization;
        _currentUser = currentUser;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
        _configuration = configuration;
        _dbContext = dbContext;
        _requestTenantContext = requestTenantContext;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        TenantId = _currentUser.GetTenantId();
    }

    public async Task<AuthDetailResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        // Validar tenantId
        if (request.TenantId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Register.InvalidTenantId"), 400);
            return null;
        }

        _requestTenantContext.SetTenantId(request.TenantId);

        var exists = await _userRepo.ExistsByNameAsync(request.TenantId, request.Name, ct);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Register.NameAlreadyExists"), 409);
            return null;
        }

        // Hash da senha
        var passwordHash = DomainExtensions.HashClientSecret(request.Secret);

        // Criar entidade de usuário
        var user = new UserEntity(request.TenantId, request.Name, request.Name, passwordHash, request.UrlImage, 0);

        // Persistir via repositório
        var created = await _userRepo.CreateAsync(user, ct);
        if (!created)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Register.FailedToCreateUser"), 500);
            return null;
        }

        // Retornar sem tokens (login separado)
        return new AuthDetailResponse
        {
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
            RoleName = user.UserRoles.FirstOrDefault()?.Role?.Name,
            TenantId = user.TenantId,
            UserId = user.Id,
            UserName = user.Name
        };
    }

    public async Task<AuthDetailResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var tenant = await _tenantRepo.GetByLoginIdentifierAsync(request.LoginIdentifier, ct);
        if (tenant is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.UserNotAssociateInTenant"), 401);
            return null;
        }

        _requestTenantContext.SetTenantId(tenant.Id);

        var user = await _userRepo.GetByNormalizedLoginAsync(tenant.Id, request.LoginIdentifier, ct);
        if (user is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Login.InvalidCredentials"), 401);
            return null;
        }

        if (!DomainExtensions.VerifyClientSecret(user.PasswordHash, request.Password))
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Login.InvalidCredentials"), 401);
            return null;
        }

        // Obter AppId das roles do usuário
        var userRole = user.UserRoles?.FirstOrDefault();
        if (userRole is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Login.UserWithoutRole"), 403);
            return null;
        }

        // Gera tokens (usa chave RSA do tenant)
        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, ct);
        if (accessToken.Token is null)
        {
            // Erro já notificado
            return null;
        }

        // Atualizar LastAccessAt do usuário (não bloquear login em caso de falha)
        try
        {
            user.UpdateLastAccess();
            await _userRepo.UpdateAsync(user, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao atualizar LastAccessAt para o usuário {UserId}", user.Id);
        }

        var issueResult = await _refreshTokenService.IssueAsync(user.TenantId, userRole.AppId, user.Id, ct);

        return new AuthDetailResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = issueResult.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = issueResult.Entity.ExpiresAt,
            TenantId = user.TenantId,
            TenantName = user.Tenant.Name,
            AppId = userRole.AppId,
            AppName = userRole.App?.Name,
            UserId = user.Id,
            UserName = user.Name,
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
            RoleName = user.UserRoles.FirstOrDefault()?.Role?.Name
        };
    }

    public async Task<AuthDetailResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        if (request.TenantId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Refresh.InvalidTenantId"), 400);
            return null;
        }

        _requestTenantContext.SetTenantId(request.TenantId);

        var rotateResult = await _refreshTokenService.RotateAsync(request.RefreshToken, request.TenantId, ct);
        if (rotateResult is null)
            return null;

        var user = await _userRepo.GetByIdAsync(request.TenantId, rotateResult.OldEntity.UserId, ct);
        if (user is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Refresh.UserNotFound"), 410);
            return null;
        }

        var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user, ct);

        return new AuthDetailResponse
        {
            AccessToken = accessToken.Token,
            RefreshToken = rotateResult.NewToken,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshTokenExpiresAt = rotateResult.NewEntity.ExpiresAt,
            RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
            RoleName = user.UserRoles.FirstOrDefault()?.Role?.Name,
            TenantId = user.TenantId,
            TenantName = user.Tenant.Name,
            UserId = user.Id,
            UserName = user.Name
        };
    }

    public async Task LogoutAsync(RevokeRequest request, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId();
        var tenantId = _currentUser.GetTenantId();

        if (userId <= 0 || tenantId <= 0)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Auth.Logout.InvalidContext"), 401);
            return;
        }

        await _refreshTokenService.RevokeAllAsync(userId, tenantId, userId, ct);

        _logger.LogInformation("Logout realizado para user {UserId} no tenant {TenantId}. Motivo: {Reason}",
            userId, tenantId, request.Reason);
    }

}
