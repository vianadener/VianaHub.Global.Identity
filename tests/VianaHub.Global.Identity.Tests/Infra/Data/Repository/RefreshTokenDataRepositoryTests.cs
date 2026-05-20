using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class RefreshTokenDataRepositoryTests
{
    private const int TenantId = 1;
    private const int AppId = 1;
    private const int UserId = 10;
    private static readonly byte[] TokenHash = new byte[] { 1, 2, 3 };

    private RefreshTokenDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private RefreshTokenDataRepository CreateSutWithData(params RefreshTokenEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.RefreshTokens.AddRange(entities);
        context.SaveChanges();
        return new RefreshTokenDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "RefreshTokenDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "RefreshTokenDataRepository - Deve implementar IRefreshTokenDataRepository")]
    [Trait("Infra.Data", "")]
    public void RefreshTokenDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IRefreshTokenDataRepository>(sut);
    }

    #endregion

    #region GetByTokenHashAsync

    [Fact(DisplayName = "GetByTokenHashAsync - Deve retornar token quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByTokenHashAsync_Encontrado_DeveRetornar()
    {
        var entity = new RefreshTokenEntity(TenantId, AppId, UserId, TokenHash, DateTime.UtcNow.AddHours(1), 1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByTokenHashAsync(TokenHash, TenantId, default);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "GetByTokenHashAsync - Deve retornar null quando não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByTokenHashAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByTokenHashAsync(new byte[] { 99 }, TenantId, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByUserAsync

    [Fact(DisplayName = "GetByUserAsync - Deve retornar tokens do usuário")]
    [Trait("Infra.Data", "")]
    public async Task GetByUserAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            new RefreshTokenEntity(TenantId, AppId, UserId, new byte[] { 1, 2, 3 }, DateTime.UtcNow.AddHours(1), 1),
            new RefreshTokenEntity(TenantId, AppId, UserId, new byte[] { 4, 5, 6 }, DateTime.UtcNow.AddHours(1), 1));

        var result = await sut.GetByUserAsync(UserId, TenantId, default);

        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetByUserAsync - Deve retornar lista vazia quando não há tokens")]
    [Trait("Infra.Data", "")]
    public async Task GetByUserAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetByUserAsync(UserId, TenantId, default);

        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir refresh token com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new RefreshTokenEntity(TenantId, AppId, UserId, new byte[] { 7, 8, 9 }, DateTime.UtcNow.AddHours(1), 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region RevokeAsync

    [Fact(DisplayName = "RevokeAsync - Deve revogar refresh token com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task RevokeAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new RefreshTokenEntity(TenantId, AppId, UserId, TokenHash, DateTime.UtcNow.AddHours(1), 1);
        context.RefreshTokens.Add(entity);
        await context.SaveChangesAsync();
        var sut = new RefreshTokenDataRepository(context);

        entity.Revoke(1);
        var result = await sut.RevokeAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region RevokeAllByUserAsync

    [Fact(DisplayName = "RevokeAllByUserAsync - Deve revogar todos os tokens do usuário")]
    [Trait("Infra.Data", "")]
    public async Task RevokeAllByUserAsync_Sucesso_DeveRetornarQuantidade()
    {
        var context = RepositoryTestHelper.CreateContext();
        context.RefreshTokens.AddRange(
            new RefreshTokenEntity(TenantId, AppId, UserId, new byte[] { 1, 2 }, DateTime.UtcNow.AddHours(1), 1),
            new RefreshTokenEntity(TenantId, AppId, UserId, new byte[] { 3, 4 }, DateTime.UtcNow.AddHours(1), 1));
        await context.SaveChangesAsync();
        var sut = new RefreshTokenDataRepository(context);

        var result = await sut.RevokeAllByUserAsync(UserId, TenantId, 1, default);

        Assert.Equal(2, result);
    }

    [Fact(DisplayName = "RevokeAllByUserAsync - Deve retornar 0 quando não há tokens ativos")]
    [Trait("Infra.Data", "")]
    public async Task RevokeAllByUserAsync_SemTokensAtivos_DeveRetornarZero()
    {
        var sut = CreateSut();

        var result = await sut.RevokeAllByUserAsync(UserId, TenantId, 1, default);

        Assert.Equal(0, result);
    }

    #endregion
}
