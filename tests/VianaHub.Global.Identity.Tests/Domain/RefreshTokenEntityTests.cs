using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Tests.Domain;

public class RefreshTokenEntityTests
{
    private static byte[] ValidHash => [1, 2, 3, 4, 5];

    private static RefreshTokenEntity BuildToken(
        int tenantId = 1,
        int appId = 2,
        int userId = 10,
        byte[]? tokenHash = null,
        DateTime? expiresAt = null,
        int createdBy = 10)
        => new(tenantId, appId, userId, tokenHash ?? ValidHash, expiresAt ?? DateTime.UtcNow.AddDays(7), createdBy);

    #region Constructor

    [Fact(DisplayName = "Constructor - Deve criar entidade com dados válidos")]
    [Trait("Domain", "")]
    public void Constructor_ComDadosValidos_DeveCriarEntidade()
    {
        var expires = DateTime.UtcNow.AddDays(7);
        var entity = BuildToken(expiresAt: expires);

        Assert.Equal(1, entity.TenantId);
        Assert.Equal(2, entity.AppId);
        Assert.Equal(10, entity.UserId);
        Assert.Equal(expires, entity.ExpiresAt);
        Assert.Null(entity.RevokedAt);
    }

    [Theory(DisplayName = "Constructor - Deve lançar ArgumentException quando IDs são inválidos")]
    [Trait("Domain", "")]
    [InlineData(0, 2, 10)]
    [InlineData(1, 0, 10)]
    [InlineData(1, 2, 0)]
    public void Constructor_ComIdsInvalidos_DeveLancarArgumentException(int tenantId, int appId, int userId)
    {
        Assert.Throws<ArgumentException>(() =>
            new RefreshTokenEntity(tenantId, appId, userId, ValidHash, DateTime.UtcNow.AddDays(1), userId));
    }

    [Fact(DisplayName = "Constructor - Deve lançar ArgumentException quando TokenHash é nulo")]
    [Trait("Domain", "")]
    public void Constructor_ComTokenHashNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new RefreshTokenEntity(1, 2, 10, null!, DateTime.UtcNow.AddDays(1), 10));
    }

    #endregion

    #region IsActive

    [Fact(DisplayName = "IsActive - Deve retornar true quando token não foi revogado e não está expirado")]
    [Trait("Domain", "")]
    public void IsActive_ComTokenNaoRevogadoENaoExpirado_DeveRetornarTrue()
    {
        var entity = BuildToken();
        Assert.True(entity.IsActive());
    }

    [Fact(DisplayName = "IsActive - Deve retornar false quando token está expirado")]
    [Trait("Domain", "")]
    public void IsActive_ComTokenExpirado_DeveRetornarFalse()
    {
        var entity = BuildToken(expiresAt: DateTime.UtcNow.AddSeconds(-1));
        Assert.False(entity.IsActive());
    }

    [Fact(DisplayName = "IsActive - Deve retornar false após revogação")]
    [Trait("Domain", "")]
    public void IsActive_AposRevoke_DeveRetornarFalse()
    {
        var entity = BuildToken();
        entity.Revoke(10);

        Assert.False(entity.IsActive());
    }

    #endregion

    #region Revoke

    [Fact(DisplayName = "Revoke - Deve definir RevokedAt e RevokedBy")]
    [Trait("Domain", "")]
    public void Revoke_DeveDefinirRevokedAtERevokedBy()
    {
        var entity = BuildToken();
        entity.Revoke(revokedBy: 99);

        Assert.NotNull(entity.RevokedAt);
        Assert.Equal(99, entity.RevokedBy);
        Assert.False(entity.IsActive());
    }

    #endregion
}
