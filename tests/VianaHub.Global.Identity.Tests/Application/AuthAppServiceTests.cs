using AutoMapper;
using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Dto.Result;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class AuthAppServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IUserDataRepository> _userRepoMock = new();
    private readonly Mock<ITenantDataRepository> _tenantRepoMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IRequestTenantContext> _tenantContextMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();

    private const int TenantId = 1;
    private const int UserId = 10;
    private const int AppId = 2;
    private const int RoleId = 3;

    public AuthAppServiceTests()
    {
        _currentUserMock.Setup(x => x.GetTenantId()).Returns(TenantId);
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);
        _currentUserMock.Setup(x => x.GetAppId()).Returns(AppId);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
    }

    private AuthAppService CreateSut()
    {
        var jwtSettings = new JwtSettings();
        var jwtOptions = Options.Create(jwtSettings);

        var dbContextOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new IdentityDbContext(dbContextOptions);

        return new AuthAppService(
            _notifyMock.Object,
            _mapperMock.Object,
            _userRepoMock.Object,
            _tenantRepoMock.Object,
            _localizationMock.Object,
            _currentUserMock.Object,
            jwtOptions,
            NullLogger<AuthAppService>.Instance,
            _configurationMock.Object,
            dbContext,
            _tenantContextMock.Object,
            _jwtTokenServiceMock.Object,
            _refreshTokenServiceMock.Object);
    }

    private static TenantEntity BuildTenant(int id = TenantId)
    {
        var tenant = new TenantEntity("Tenant Test", "desc", "alias", null, null, null, 0);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(tenant, id);
        return tenant;
    }

    private static UserEntity BuildUser(int tenantId = TenantId, int userId = UserId, string login = "user@example.com", bool active = true)
    {
        var passwordHash = DomainExtensions.HashClientSecret("Senha@123");
        var user = new UserEntity(tenantId, "Test User", login, passwordHash, null, 0);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(user, userId);

        if (!active)
            user.Deactivate(0);

        return user;
    }

    private static UserEntity BuildUserWithRole(int tenantId = TenantId, int userId = UserId)
    {
        var user = BuildUser(tenantId, userId);

        var appEntity = new AppEntity(tenantId, "App Test", "Desc", 0);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(appEntity, AppId);

        var role = new RoleEntity(tenantId, AppId, "Admin", "desc", 0);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(role, RoleId);

        var userRole = new UserRoleEntity(tenantId, AppId, userId, RoleId);
        typeof(UserRoleEntity)
            .GetProperty("App")!
            .SetValue(userRole, appEntity);
        typeof(UserRoleEntity)
            .GetProperty("Role")!
            .SetValue(userRole, role);

        var userRoles = typeof(UserEntity)
            .GetField("_userRoles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        ((List<UserRoleEntity>)userRoles.GetValue(user)!).Add(userRole);

        var tenantEntity = BuildTenant(tenantId);
        typeof(UserEntity)
            .GetProperty("Tenant")!
            .SetValue(user, tenantEntity);

        return user;
    }

    private static RefreshTokenEntity BuildRefreshTokenEntity()
    {
        var tokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("rt-token"));
        var entity = new RefreshTokenEntity(TenantId, AppId, UserId, tokenHash, DateTime.UtcNow.AddDays(7), 0);
        return entity;
    }

    #region RegisterAsync

    [Fact(DisplayName = "RegisterAsync - Deve retornar null e notificar quando TenantId inválido")]
    [Trait("Application", "")]
    public async Task RegisterAsync_TenantIdInvalido_DeveNotificar400ERetornarNull()
    {
        var request = new RegisterRequest { TenantId = 0, Name = "user", Secret = "Senha@123" };
        var sut = CreateSut();

        var result = await sut.RegisterAsync(request, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "RegisterAsync - Deve retornar null e notificar quando nome já existe")]
    [Trait("Application", "")]
    public async Task RegisterAsync_NomeJaExiste_DeveNotificar409ERetornarNull()
    {
        var request = new RegisterRequest { TenantId = TenantId, Name = "user existente", Secret = "Senha@123" };
        _userRepoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.RegisterAsync(request, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 409), Times.Once);
    }

    [Fact(DisplayName = "RegisterAsync - Deve retornar null quando falha ao criar usuário")]
    [Trait("Application", "")]
    public async Task RegisterAsync_FalhaNaCriacao_DeveNotificar500ERetornarNull()
    {
        var request = new RegisterRequest { TenantId = TenantId, Name = "novo user", Secret = "Senha@123" };
        _userRepoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(false);
        _userRepoMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.RegisterAsync(request, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 500), Times.Once);
    }

    [Fact(DisplayName = "RegisterAsync - Deve registrar usuário com sucesso")]
    [Trait("Application", "")]
    public async Task RegisterAsync_Sucesso_DeveRetornarAuthDetailResponse()
    {
        var request = new RegisterRequest { TenantId = TenantId, Name = "novo user", Secret = "Senha@123" };
        _userRepoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(false);
        _userRepoMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.RegisterAsync(request, default);

        Assert.NotNull(result);
        _userRepoMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Once);
    }

    #endregion

    #region LoginAsync

    [Fact(DisplayName = "LoginAsync - Deve retornar null e notificar quando tenant não encontrado")]
    [Trait("Application", "")]
    public async Task LoginAsync_TenantNaoEncontrado_DeveNotificar401ERetornarNull()
    {
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync((TenantEntity)null);

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = "nao@existe.com", Password = "Senha@123" }, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
    }

    [Fact(DisplayName = "LoginAsync - Deve retornar null e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task LoginAsync_UsuarioNaoEncontrado_DeveNotificar401ERetornarNull()
    {
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(TenantId, It.IsAny<string>(), default))
                     .ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = "user@example.com", Password = "Senha@123" }, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
    }

    [Fact(DisplayName = "LoginAsync - Deve retornar null e notificar quando senha incorreta")]
    [Trait("Application", "")]
    public async Task LoginAsync_SenhaIncorreta_DeveNotificar401ERetornarNull()
    {
        var user = BuildUser();
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(TenantId, It.IsAny<string>(), default))
                     .ReturnsAsync(user);

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = "user@example.com", Password = "SenhaErrada" }, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
    }

    [Fact(DisplayName = "LoginAsync - Deve retornar null e notificar quando usuário não tem role")]
    [Trait("Application", "")]
    public async Task LoginAsync_SemRole_DeveNotificar403ERetornarNull()
    {
        var user = BuildUser();
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(TenantId, It.IsAny<string>(), default))
                     .ReturnsAsync(user);

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = "user@example.com", Password = "Senha@123" }, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 403), Times.Once);
    }

    [Fact(DisplayName = "LoginAsync - Deve retornar null quando geração do access token falha")]
    [Trait("Application", "")]
    public async Task LoginAsync_AccessTokenFalha_DeveRetornarNull()
    {
        var user = BuildUserWithRole();
        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(TenantId, It.IsAny<string>(), default))
                     .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.UpdateAsync(user, default)).ReturnsAsync(true);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(user, default))
                            .ReturnsAsync(((string)null, DateTime.UtcNow.AddHours(1)));

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = "user@example.com", Password = "Senha@123" }, default);

        Assert.Null(result);
    }

    [Fact(DisplayName = "LoginAsync - Deve retornar AuthDetailResponse com sucesso")]
    [Trait("Application", "")]
    public async Task LoginAsync_Sucesso_DeveRetornarAuthDetailResponse()
    {
        var user = BuildUserWithRole();
        var refreshEntity = BuildRefreshTokenEntity();

        _tenantRepoMock.Setup(x => x.GetByLoginIdentifierAsync(It.IsAny<string>(), default))
                       .ReturnsAsync(BuildTenant());
        _userRepoMock.Setup(x => x.GetByNormalizedLoginAsync(TenantId, It.IsAny<string>(), default))
                     .ReturnsAsync(user);
        _userRepoMock.Setup(x => x.UpdateAsync(user, default)).ReturnsAsync(true);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(user, default))
                            .ReturnsAsync(("access.token.jwt", DateTime.UtcNow.AddHours(1)));
        _refreshTokenServiceMock.Setup(x => x.IssueAsync(TenantId, AppId, UserId, default))
                                .ReturnsAsync(new RefreshTokenIssueResult
                                {
                                    Token = "refresh-token",
                                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                                    Entity = refreshEntity
                                });

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest { LoginIdentifier = "user@example.com", Password = "Senha@123" }, default);

        Assert.NotNull(result);
        Assert.Equal("access.token.jwt", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(TenantId, result.TenantId);
        Assert.Equal(UserId, result.UserId);
    }

    #endregion

    #region RefreshAsync

    [Fact(DisplayName = "RefreshAsync - Deve retornar null e notificar quando TenantId inválido")]
    [Trait("Application", "")]
    public async Task RefreshAsync_TenantIdInvalido_DeveNotificar400ERetornarNull()
    {
        var sut = CreateSut();
        var result = await sut.RefreshAsync(new RefreshRequest { TenantId = 0, RefreshToken = "token" }, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "RefreshAsync - Deve retornar null quando rotação do token falha")]
    [Trait("Application", "")]
    public async Task RefreshAsync_RotacaoFalha_DeveRetornarNull()
    {
        _refreshTokenServiceMock.Setup(x => x.RotateAsync(It.IsAny<string>(), TenantId, default))
                                .ReturnsAsync((RefreshTokenRotateResult)null);

        var sut = CreateSut();
        var result = await sut.RefreshAsync(new RefreshRequest { TenantId = TenantId, RefreshToken = "token-invalido" }, default);

        Assert.Null(result);
    }

    [Fact(DisplayName = "RefreshAsync - Deve retornar null e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task RefreshAsync_UsuarioNaoEncontrado_DeveNotificar410ERetornarNull()
    {
        var oldEntity = BuildRefreshTokenEntity();
        var newEntity = BuildRefreshTokenEntity();
        var rotateResult = new RefreshTokenRotateResult { OldEntity = oldEntity, NewToken = "new-token", NewEntity = newEntity };

        _refreshTokenServiceMock.Setup(x => x.RotateAsync(It.IsAny<string>(), TenantId, default))
                                .ReturnsAsync(rotateResult);
        _userRepoMock.Setup(x => x.GetByIdAsync(TenantId, UserId, default))
                     .ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.RefreshAsync(new RefreshRequest { TenantId = TenantId, RefreshToken = "token" }, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
    }

    [Fact(DisplayName = "RefreshAsync - Deve retornar AuthDetailResponse com sucesso")]
    [Trait("Application", "")]
    public async Task RefreshAsync_Sucesso_DeveRetornarAuthDetailResponse()
    {
        var user = BuildUserWithRole();
        var oldEntity = BuildRefreshTokenEntity();
        var newEntity = BuildRefreshTokenEntity();
        var rotateResult = new RefreshTokenRotateResult { OldEntity = oldEntity, NewToken = "new-refresh-token", NewEntity = newEntity };

        _refreshTokenServiceMock.Setup(x => x.RotateAsync(It.IsAny<string>(), TenantId, default))
                                .ReturnsAsync(rotateResult);
        _userRepoMock.Setup(x => x.GetByIdAsync(TenantId, UserId, default))
                     .ReturnsAsync(user);
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessTokenAsync(user, default))
                            .ReturnsAsync(("new.access.token", DateTime.UtcNow.AddHours(1)));

        var sut = CreateSut();
        var result = await sut.RefreshAsync(new RefreshRequest { TenantId = TenantId, RefreshToken = "old-token" }, default);

        Assert.NotNull(result);
        Assert.Equal("new.access.token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
    }

    #endregion

    #region LogoutAsync

    [Fact(DisplayName = "LogoutAsync - Deve notificar quando contexto inválido")]
    [Trait("Application", "")]
    public async Task LogoutAsync_ContextoInvalido_DeveNotificar401()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(0);

        var sut = CreateSut();
        await sut.LogoutAsync(new RevokeRequest { Reason = "manual" }, default);

        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
        _refreshTokenServiceMock.Verify(x => x.RevokeAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), default), Times.Never);
    }

    [Fact(DisplayName = "LogoutAsync - Deve revogar todos os tokens com sucesso")]
    [Trait("Application", "")]
    public async Task LogoutAsync_Sucesso_DeveRevogarTodosOsTokens()
    {
        _refreshTokenServiceMock.Setup(x => x.RevokeAllAsync(UserId, TenantId, UserId, default))
                                .ReturnsAsync(2);

        var sut = CreateSut();
        await sut.LogoutAsync(new RevokeRequest { Reason = "manual" }, default);

        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _refreshTokenServiceMock.Verify(x => x.RevokeAllAsync(UserId, TenantId, UserId, default), Times.Once);
    }

    #endregion
}
