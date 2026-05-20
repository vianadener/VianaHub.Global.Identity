using AutoMapper;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Response.Jwt;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class JwtKeyAppServiceTests
{
    private readonly Mock<IJwtKeyDataRepository> _repoMock = new();
    private readonly Mock<IJwtKeyDomainService> _domainMock = new();
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private const int TenantId = 1;
    private const int UserId = 10;

    public JwtKeyAppServiceTests()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
    }

    private JwtKeyAppService CreateSut() => new(
        _repoMock.Object,
        _domainMock.Object,
        _notifyMock.Object,
        _mapperMock.Object,
        _currentUserMock.Object,
        _localizationMock.Object);

    private static JwtKeyEntity BuildKey(int id = 1, bool revoked = false)
    {
        var entity = new JwtKeyEntity(TenantId, "public-key", "encrypted-private-key", UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        if (revoked)
            entity.Revoke("Motivo", UserId);
        return entity;
    }

    #region GetByTenantAsync

    [Fact(DisplayName = "GetByTenantAsync - Deve retornar lista de chaves do tenant")]
    [Trait("Application", "")]
    public async Task GetByTenantAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<JwtKeyEntity> { BuildKey(1), BuildKey(2) };
        var mapped = new List<JwtKeyResponse> { new() { Id = 1 }, new() { Id = 2 } };
        _repoMock.Setup(x => x.GetByTenantAsync(TenantId, default)).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<IEnumerable<JwtKeyResponse>>(entities)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetByTenantAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetByTenantAsync - Deve retornar lista vazia quando não há chaves")]
    [Trait("Application", "")]
    public async Task GetByTenantAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetByTenantAsync(TenantId, default)).ReturnsAsync([]);
        _mapperMock.Setup(x => x.Map<IEnumerable<JwtKeyResponse>>(It.IsAny<IEnumerable<JwtKeyEntity>>())).Returns([]);

        var sut = CreateSut();
        var result = await sut.GetByTenantAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetActiveKeyAsync

    [Fact(DisplayName = "GetActiveKeyAsync - Deve retornar chave ativa mapeada")]
    [Trait("Application", "")]
    public async Task GetActiveKeyAsync_Sucesso_DeveRetornarChaveAtiva()
    {
        var entity = BuildKey(1);
        var mapped = new JwtKeyResponse { Id = 1 };
        _repoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<JwtKeyResponse>(entity)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetActiveKeyAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.IsType<JwtKeyResponse>(result);
    }

    [Fact(DisplayName = "GetActiveKeyAsync - Deve retornar null quando não há chave ativa")]
    [Trait("Application", "")]
    public async Task GetActiveKeyAsync_SemChaveAtiva_DeveRetornarNull()
    {
        _repoMock.Setup(x => x.GetActiveKeyAsync(TenantId, default)).ReturnsAsync((JwtKeyEntity)null);

        var sut = CreateSut();
        var result = await sut.GetActiveKeyAsync(TenantId, default);

        Assert.Null(result);
    }

    #endregion

    #region CreateInitialIfNotExistsAsync

    [Fact(DisplayName = "CreateInitialIfNotExistsAsync - Deve retornar true quando chave ativa já existe")]
    [Trait("Application", "")]
    public async Task CreateInitialIfNotExistsAsync_ChaveJaExiste_DeveRetornarTrue()
    {
        _repoMock.Setup(x => x.HasActiveKeyAsync(TenantId, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateInitialIfNotExistsAsync(TenantId, default);

        Assert.True(result);
        _domainMock.Verify(x => x.EnsureKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), default), Times.Never);
    }

    [Fact(DisplayName = "CreateInitialIfNotExistsAsync - Deve criar chave inicial quando não existe")]
    [Trait("Application", "")]
    public async Task CreateInitialIfNotExistsAsync_SemChave_DeveCriarERetornarTrue()
    {
        var entity = BuildKey(1);
        _repoMock.Setup(x => x.HasActiveKeyAsync(TenantId, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.EnsureKeyExistsAsync(TenantId, UserId, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.CreateInitialIfNotExistsAsync(TenantId, default);

        Assert.True(result);
        _domainMock.Verify(x => x.EnsureKeyExistsAsync(TenantId, UserId, default), Times.Once);
    }

    [Fact(DisplayName = "CreateInitialIfNotExistsAsync - Deve retornar false quando domínio retorna null")]
    [Trait("Application", "")]
    public async Task CreateInitialIfNotExistsAsync_DominioRetornaNull_DeveRetornarFalse()
    {
        _repoMock.Setup(x => x.HasActiveKeyAsync(TenantId, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.EnsureKeyExistsAsync(TenantId, UserId, default)).ReturnsAsync((JwtKeyEntity)null);

        var sut = CreateSut();
        var result = await sut.CreateInitialIfNotExistsAsync(TenantId, default);

        Assert.False(result);
    }

    #endregion

    #region RevokeAsync

    [Fact(DisplayName = "RevokeAsync - Deve revogar chave com sucesso")]
    [Trait("Application", "")]
    public async Task RevokeAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.RevokeAsync(1, "Motivo válido", UserId, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.RevokeAsync(1, "Motivo válido", default);

        Assert.True(result);
        _domainMock.Verify(x => x.RevokeAsync(1, "Motivo válido", UserId, default), Times.Once);
    }

    [Fact(DisplayName = "RevokeAsync - Deve retornar false e notificar quando chave não encontrada")]
    [Trait("Application", "")]
    public async Task RevokeAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JwtKeyEntity)null);

        var sut = CreateSut();
        var result = await sut.RevokeAsync(99, "Motivo", default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.RevokeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), default), Times.Never);
    }

    [Fact(DisplayName = "RevokeAsync - Deve retornar false e notificar quando chave já está revogada")]
    [Trait("Application", "")]
    public async Task RevokeAsync_JaRevogada_DeveRetornarFalseENotificar()
    {
        var entity = BuildKey(1, revoked: true);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.RevokeAsync(1, "Motivo", default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.RevokeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), default), Times.Never);
    }

    [Fact(DisplayName = "RevokeAsync - Deve retornar false e notificar quando motivo está vazio")]
    [Trait("Application", "")]
    public async Task RevokeAsync_MotivoVazio_DeveRetornarFalseENotificar()
    {
        var entity = BuildKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.RevokeAsync(1, string.Empty, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.RevokeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), default), Times.Never);
    }

    [Fact(DisplayName = "RevokeAsync - Deve retornar false quando domínio falha")]
    [Trait("Application", "")]
    public async Task RevokeAsync_DominioFalha_DeveRetornarFalse()
    {
        var entity = BuildKey(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.RevokeAsync(1, "Motivo válido", UserId, default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.RevokeAsync(1, "Motivo válido", default);

        Assert.False(result);
    }

    #endregion
}
