using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class RefreshTokenServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IRefreshTokenDataRepository> _refreshRepoMock = new();
    private readonly Mock<IRefreshTokenHasher> _hasherMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public RefreshTokenServiceTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
        _hasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns(new byte[] { 1, 2, 3, 4 });
    }

    private RefreshTokenService CreateSut(JwtSettings settings = null)
    {
        settings ??= new JwtSettings { RefreshTokenExpirationDays = 7 };
        return new RefreshTokenService(
            _notifyMock.Object,
            _refreshRepoMock.Object,
            _hasherMock.Object,
            _localizationMock.Object,
            NullLogger<RefreshTokenService>.Instance,
            Options.Create(settings));
    }

    private static RefreshTokenEntity BuildRefreshToken(bool active = true)
    {
        var expiresAt = active ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(-1);
        return new RefreshTokenEntity(TenantId, AppId, UserId, new byte[] { 1, 2, 3, 4 }, expiresAt, UserId);
    }

    #region IssueAsync

    [Fact(DisplayName = "IssueAsync - Deve emitir refresh token com sucesso")]
    [Trait("Application", "")]
    public async Task IssueAsync_Sucesso_DeveRetornarResult()
    {
        _refreshRepoMock.Setup(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.IssueAsync(TenantId, AppId, UserId, default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        Assert.NotNull(result.Entity);
        _refreshRepoMock.Verify(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "IssueAsync - Deve respeitar expiração configurada")]
    [Trait("Application", "")]
    public async Task IssueAsync_DeveRespeitarExpiracaoConfigurada()
    {
        var settings = new JwtSettings { RefreshTokenExpirationDays = 30 };
        _refreshRepoMock.Setup(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut(settings);
        var before = DateTime.UtcNow;
        var result = await sut.IssueAsync(TenantId, AppId, UserId, default);

        Assert.InRange(result.ExpiresAt, before.AddDays(29), before.AddDays(31));
    }

    #endregion

    #region RotateAsync

    [Fact(DisplayName = "RotateAsync - Deve rotacionar token ativo com sucesso")]
    [Trait("Application", "")]
    public async Task RotateAsync_TokenAtivo_DeveRetornarNovoToken()
    {
        var existing = BuildRefreshToken(active: true);
        _refreshRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), TenantId, default)).ReturnsAsync(existing);
        _refreshRepoMock.Setup(x => x.RevokeAsync(existing, default)).ReturnsAsync(true);
        _refreshRepoMock.Setup(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.RotateAsync("valid-raw-token", TenantId, default);

        Assert.NotNull(result);
        Assert.NotEmpty(result.NewToken);
        Assert.NotNull(result.OldEntity);
        Assert.NotNull(result.NewEntity);
        _refreshRepoMock.Verify(x => x.RevokeAsync(existing, default), Times.Once);
        _refreshRepoMock.Verify(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "RotateAsync - Deve retornar null e notificar quando token não encontrado")]
    [Trait("Application", "")]
    public async Task RotateAsync_TokenNaoEncontrado_DeveRetornarNullENotificar()
    {
        _refreshRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), TenantId, default))
            .ReturnsAsync((RefreshTokenEntity)null);

        var sut = CreateSut();
        var result = await sut.RotateAsync("invalid-token", TenantId, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
        _refreshRepoMock.Verify(x => x.RevokeAsync(It.IsAny<RefreshTokenEntity>(), default), Times.Never);
        _refreshRepoMock.Verify(x => x.CreateAsync(It.IsAny<RefreshTokenEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "RotateAsync - Deve retornar null e notificar quando token está expirado")]
    [Trait("Application", "")]
    public async Task RotateAsync_TokenExpirado_DeveRetornarNullENotificar()
    {
        var expiredToken = BuildRefreshToken(active: false);
        _refreshRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), TenantId, default)).ReturnsAsync(expiredToken);

        var sut = CreateSut();
        var result = await sut.RotateAsync("expired-token", TenantId, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
        _refreshRepoMock.Verify(x => x.RevokeAsync(It.IsAny<RefreshTokenEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "RotateAsync - Deve retornar null e notificar quando token foi revogado")]
    [Trait("Application", "")]
    public async Task RotateAsync_TokenRevogado_DeveRetornarNullENotificar()
    {
        var revokedToken = BuildRefreshToken(active: true);
        revokedToken.Revoke(UserId);

        _refreshRepoMock.Setup(x => x.GetByTokenHashAsync(It.IsAny<byte[]>(), TenantId, default)).ReturnsAsync(revokedToken);

        var sut = CreateSut();
        var result = await sut.RotateAsync("revoked-token", TenantId, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 401), Times.Once);
    }

    #endregion

    #region RevokeAllAsync

    [Fact(DisplayName = "RevokeAllAsync - Deve revogar todos os tokens do usuário com sucesso")]
    [Trait("Application", "")]
    public async Task RevokeAllAsync_Sucesso_DeveRetornarQuantidadeRevogada()
    {
        _refreshRepoMock.Setup(x => x.RevokeAllByUserAsync(UserId, TenantId, UserId, default)).ReturnsAsync(3);

        var sut = CreateSut();
        var result = await sut.RevokeAllAsync(UserId, TenantId, UserId, default);

        Assert.Equal(3, result);
        _refreshRepoMock.Verify(x => x.RevokeAllByUserAsync(UserId, TenantId, UserId, default), Times.Once);
    }

    [Fact(DisplayName = "RevokeAllAsync - Deve retornar zero quando não há tokens para revogar")]
    [Trait("Application", "")]
    public async Task RevokeAllAsync_SemTokens_DeveRetornarZero()
    {
        _refreshRepoMock.Setup(x => x.RevokeAllByUserAsync(UserId, TenantId, UserId, default)).ReturnsAsync(0);

        var sut = CreateSut();
        var result = await sut.RevokeAllAsync(UserId, TenantId, UserId, default);

        Assert.Equal(0, result);
    }

    #endregion
}
