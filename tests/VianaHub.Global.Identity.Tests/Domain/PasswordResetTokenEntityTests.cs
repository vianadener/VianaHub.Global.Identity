using VianaHub.Global.Identity.Domain.Entities;

namespace VianaHub.Global.Identity.Tests.Domain;

public class PasswordResetTokenEntityTests
{
    private static byte[] ValidHash => [1, 2, 3, 4, 5];
    private static DateTime FutureExpiry => DateTime.UtcNow.AddMinutes(15);

    private static PasswordResetTokenEntity BuildToken(
        int tenantId = 1,
        int userId = 10,
        byte[]? tokenHash = null,
        DateTime? expiresAt = null,
        int createdBy = 0)
        => new(tenantId, userId, tokenHash ?? ValidHash, expiresAt ?? FutureExpiry, createdBy);

    #region Constructor

    [Fact(DisplayName = "Constructor - Deve criar entidade com dados válidos")]
    [Trait("Domain", "")]
    public void Constructor_ComDadosValidos_DeveCriarEntidade()
    {
        var expires = FutureExpiry;
        var entity = BuildToken(expiresAt: expires);

        Assert.Equal(1, entity.TenantId);
        Assert.Equal(10, entity.UserId);
        Assert.Equal(ValidHash, entity.TokenHash);
        Assert.Equal(expires, entity.ExpiresAt);
        Assert.False(entity.Used);
    }

    [Theory(DisplayName = "Constructor - Deve lançar ArgumentException quando IDs são inválidos")]
    [Trait("Domain", "")]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    public void Constructor_ComIdsInvalidos_DeveLancarArgumentException(int tenantId, int userId)
    {
        Assert.Throws<ArgumentException>(() =>
            new PasswordResetTokenEntity(tenantId, userId, ValidHash, FutureExpiry, 0));
    }

    [Fact(DisplayName = "Constructor - Deve lançar ArgumentException quando TokenHash é nulo")]
    [Trait("Domain", "")]
    public void Constructor_ComTokenHashNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PasswordResetTokenEntity(1, 10, null!, FutureExpiry, 0));
    }

    [Fact(DisplayName = "Constructor - Deve lançar ArgumentException quando ExpiresAt está no passado")]
    [Trait("Domain", "")]
    public void Constructor_ComExpiresAtNoPassado_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PasswordResetTokenEntity(1, 10, ValidHash, DateTime.UtcNow.AddSeconds(-1), 0));
    }

    #endregion

    #region IsValid

    [Fact(DisplayName = "IsValid - Deve retornar true quando token não foi utilizado e não está expirado")]
    [Trait("Domain", "")]
    public void IsValid_ComTokenNaoUsadoENaoExpirado_DeveRetornarTrue()
    {
        var entity = BuildToken();
        Assert.True(entity.IsValid());
    }

    [Fact(DisplayName = "IsValid - Deve retornar false após MarkAsUsed")]
    [Trait("Domain", "")]
    public void IsValid_AposMarkAsUsed_DeveRetornarFalse()
    {
        var entity = BuildToken();
        entity.MarkAsUsed(modifiedBy: 99);

        Assert.False(entity.IsValid());
    }

    #endregion

    #region MarkAsUsed

    [Fact(DisplayName = "MarkAsUsed - Deve definir Used como true e registrar ModifiedBy")]
    [Trait("Domain", "")]
    public void MarkAsUsed_DeveDefinirUsedEModifiedBy()
    {
        var entity = BuildToken();
        entity.MarkAsUsed(modifiedBy: 99);

        Assert.True(entity.Used);
        Assert.Equal(99, entity.ModifiedBy);
        Assert.NotNull(entity.ModifiedAt);
    }

    #endregion
}
