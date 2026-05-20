using VianaHub.Global.Identity.Domain.Tools.Cryptography;

namespace VianaHub.Global.Identity.Tests.Domain.Tools.Cryptography;

public class CryptoMD5Tests
{
    #region Encrypt

    [Fact(DisplayName = "Encrypt - Deve retornar hash hexadecimal não vazio para entrada válida")]
    [Trait("Domain", "")]
    public void Encrypt_EntradaValida_DeveRetornarHashHexadecimal()
    {
        var result = CryptoMD5.Encrypt("senha123");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(32, result.Length);
        Assert.Matches("^[a-f0-9]{32}$", result);
    }

    [Fact(DisplayName = "Encrypt - Deve retornar o mesmo hash para a mesma entrada")]
    [Trait("Domain", "")]
    public void Encrypt_MesmaEntrada_DeveRetornarMesmoHash()
    {
        var result1 = CryptoMD5.Encrypt("senha123");
        var result2 = CryptoMD5.Encrypt("senha123");

        Assert.Equal(result1, result2);
    }

    [Fact(DisplayName = "Encrypt - Deve retornar hashes diferentes para entradas diferentes")]
    [Trait("Domain", "")]
    public void Encrypt_EntradasDiferentes_DeveRetornarHashesDiferentes()
    {
        var result1 = CryptoMD5.Encrypt("senha123");
        var result2 = CryptoMD5.Encrypt("senha456");

        Assert.NotEqual(result1, result2);
    }

    [Fact(DisplayName = "Encrypt - Deve retornar hash para entrada vazia")]
    [Trait("Domain", "")]
    public void Encrypt_EntradaVazia_DeveRetornarHashValido()
    {
        var result = CryptoMD5.Encrypt(string.Empty);

        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact(DisplayName = "Encrypt - Deve retornar hash em letras minúsculas")]
    [Trait("Domain", "")]
    public void Encrypt_EntradaValida_DeveRetornarHashMinusculo()
    {
        var result = CryptoMD5.Encrypt("TesteCriptografia");

        Assert.Equal(result.ToLower(), result);
    }

    #endregion

    #region Verify

    [Fact(DisplayName = "Verify - Deve retornar true quando entrada corresponde ao hash")]
    [Trait("Domain", "")]
    public void Verify_EntradaCorresponde_DeveRetornarTrue()
    {
        var input = "senha123";
        var hash = CryptoMD5.Encrypt(input);

        var result = CryptoMD5.Verify(input, hash);

        Assert.True(result);
    }

    [Fact(DisplayName = "Verify - Deve retornar false quando entrada não corresponde ao hash")]
    [Trait("Domain", "")]
    public void Verify_EntradaNaoCorresponde_DeveRetornarFalse()
    {
        var hash = CryptoMD5.Encrypt("senha123");

        var result = CryptoMD5.Verify("senhaErrada", hash);

        Assert.False(result);
    }

    [Fact(DisplayName = "Verify - Deve ser case-insensitive na comparação de hash")]
    [Trait("Domain", "")]
    public void Verify_HashEmMaiusculas_DeveRetornarTrue()
    {
        var input = "senha123";
        var hash = CryptoMD5.Encrypt(input).ToUpper();

        var result = CryptoMD5.Verify(input, hash);

        Assert.True(result);
    }

    [Fact(DisplayName = "Verify - Deve retornar false para hash vazio")]
    [Trait("Domain", "")]
    public void Verify_HashVazio_DeveRetornarFalse()
    {
        var result = CryptoMD5.Verify("senha123", string.Empty);

        Assert.False(result);
    }

    #endregion
}
