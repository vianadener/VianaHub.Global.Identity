using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class PasswordResetTokenDataRepositoryTests
{
    private const int TenantId = 1;
    private const int UserId = 10;
    private static readonly byte[] TokenHash = new byte[] { 10, 20, 30 };

    private PasswordResetTokenDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private PasswordResetTokenDataRepository CreateSutWithData(params PasswordResetTokenEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.PasswordResetTokens.AddRange(entities);
        context.SaveChanges();
        return new PasswordResetTokenDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "PasswordResetTokenDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "PasswordResetTokenDataRepository - Deve implementar IPasswordResetTokenDataRepository")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokenDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IPasswordResetTokenDataRepository>(sut);
    }

    #endregion

    #region GetByTokenHashAsync

    [Fact(DisplayName = "GetByTokenHashAsync - Deve retornar token quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByTokenHashAsync_Encontrado_DeveRetornar()
    {
        var entity = new PasswordResetTokenEntity(TenantId, UserId, TokenHash, DateTime.UtcNow.AddMinutes(15), 1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByTokenHashAsync(TokenHash, default);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "GetByTokenHashAsync - Deve retornar null quando não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByTokenHashAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByTokenHashAsync(new byte[] { 99 }, default);

        Assert.Null(result);
    }

    #endregion

    #region CountRecentByUserAsync

    [Fact(DisplayName = "CountRecentByUserAsync - Deve retornar contagem de tokens recentes")]
    [Trait("Infra.Data", "")]
    public async Task CountRecentByUserAsync_Sucesso_DeveRetornarContagem()
    {
        var since = DateTime.UtcNow.AddHours(-1);
        var sut = CreateSutWithData(
            new PasswordResetTokenEntity(TenantId, UserId, new byte[] { 1, 2 }, DateTime.UtcNow.AddMinutes(15), 1),
            new PasswordResetTokenEntity(TenantId, UserId, new byte[] { 3, 4 }, DateTime.UtcNow.AddMinutes(15), 1));

        var result = await sut.CountRecentByUserAsync(UserId, TenantId, since, default);

        Assert.Equal(2, result);
    }

    [Fact(DisplayName = "CountRecentByUserAsync - Deve retornar 0 quando não há tokens recentes")]
    [Trait("Infra.Data", "")]
    public async Task CountRecentByUserAsync_SemTokensRecentes_DeveRetornarZero()
    {
        var sut = CreateSut();

        var result = await sut.CountRecentByUserAsync(UserId, TenantId, DateTime.UtcNow.AddHours(-1), default);

        Assert.Equal(0, result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir token com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new PasswordResetTokenEntity(TenantId, UserId, TokenHash, DateTime.UtcNow.AddMinutes(15), 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar token com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new PasswordResetTokenEntity(TenantId, UserId, TokenHash, DateTime.UtcNow.AddMinutes(15), 1);
        context.PasswordResetTokens.Add(entity);
        await context.SaveChangesAsync();
        var sut = new PasswordResetTokenDataRepository(context);

        entity.MarkAsUsed(1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
