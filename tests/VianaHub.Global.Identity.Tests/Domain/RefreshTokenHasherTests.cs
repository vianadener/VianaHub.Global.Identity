using VianaHub.Global.Identity.Domain.Services;

namespace VianaHub.Global.Identity.Tests.Domain;

public class RefreshTokenHasherTests
{
    private readonly RefreshTokenHasher _sut = new();

    #region Hash

    [Fact(DisplayName = "Hash - Deve retornar hash SHA256 para token válido")]
    [Trait("Domain", "")]
    public void Hash_TokenValido_DeveRetornarHash()
    {
        var token = "meu-refresh-token-valido";

        var result = _sut.Hash(token);

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact(DisplayName = "Hash - Deve retornar o mesmo hash para o mesmo token")]
    [Trait("Domain", "")]
    public void Hash_MesmoToken_DeveRetornarMesmoHash()
    {
        var token = "token-deterministico";

        var hash1 = _sut.Hash(token);
        var hash2 = _sut.Hash(token);

        Assert.Equal(hash1, hash2);
    }

    [Fact(DisplayName = "Hash - Deve retornar hashes diferentes para tokens distintos")]
    [Trait("Domain", "")]
    public void Hash_TokensDiferentes_DeveRetornarHashesDiferentes()
    {
        var hash1 = _sut.Hash("token-a");
        var hash2 = _sut.Hash("token-b");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact(DisplayName = "Hash - Deve lançar exceção quando token é nulo")]
    [Trait("Domain", "")]
    public void Hash_TokenNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Hash(null!));
    }

    [Fact(DisplayName = "Hash - Deve lançar exceção quando token é string vazia")]
    [Trait("Domain", "")]
    public void Hash_TokenVazio_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Hash(string.Empty));
    }

    [Fact(DisplayName = "Hash - Deve lançar exceção quando token é apenas espaços")]
    [Trait("Domain", "")]
    public void Hash_TokenEspacos_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.Hash("   "));
    }

    #endregion
}
