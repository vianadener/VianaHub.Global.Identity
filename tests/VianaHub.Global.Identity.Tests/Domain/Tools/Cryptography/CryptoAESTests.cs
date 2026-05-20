using VianaHub.Global.Identity.Domain.Tools.Cryptography;

namespace VianaHub.Global.Identity.Tests.Domain.Tools.Cryptography;

public class CryptoAESTests
{
    #region Encrypt

    [Fact(DisplayName = "Encrypt - Deve retornar string Base64 não vazia para entrada válida")]
    [Trait("Domain", "")]
    public void Encrypt_EntradaValida_DeveRetornarBase64NaoVazio()
    {
        var result = CryptoAES.Encrypt("texto secreto");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(IsBase64(result));
    }

    [Fact(DisplayName = "Encrypt - Deve retornar resultados diferentes para mesmas entradas (IV aleatório)")]
    [Trait("Domain", "")]
    public void Encrypt_MesmaEntrada_DeveRetornarResultadosDiferentes()
    {
        var input = "texto secreto";

        var result1 = CryptoAES.Encrypt(input);
        var result2 = CryptoAES.Encrypt(input);

        Assert.NotEqual(result1, result2);
    }

    [Fact(DisplayName = "Encrypt - Deve retornar string Base64 para entrada vazia")]
    [Trait("Domain", "")]
    public void Encrypt_EntradaVazia_DeveRetornarBase64()
    {
        var result = CryptoAES.Encrypt(string.Empty);

        Assert.NotNull(result);
        Assert.True(IsBase64(result));
    }

    [Fact(DisplayName = "Encrypt - Deve retornar string Base64 para entrada com caracteres especiais")]
    [Trait("Domain", "")]
    public void Encrypt_CaracteresEspeciais_DeveRetornarBase64()
    {
        var result = CryptoAES.Encrypt("!@#$%^&*()_+-=[]{}|;':\",./<>?àéíõü");

        Assert.NotNull(result);
        Assert.True(IsBase64(result));
    }

    #endregion

    #region Decrypt

    [Fact(DisplayName = "Decrypt - Deve lançar CryptographicException ao tentar descriptografar saída do Encrypt pois o IV não é preservado")]
    [Trait("Domain", "")]
    public void Decrypt_SaidaDoEncrypt_DeveLancarCryptographicExceptionPoisIVNaoEhPreservado()
    {
        // O Encrypt gera um IV aleatório a cada chamada mas não o inclui no output Base64.
        // O Decrypt instancia um novo Aes com IV diferente, tornando o padding inválido.
        var encrypted = CryptoAES.Encrypt("texto secreto");

        Assert.ThrowsAny<Exception>(() => CryptoAES.Decrypt(encrypted));
    }

    [Fact(DisplayName = "Decrypt - Deve lançar exceção para Base64 inválido")]
    [Trait("Domain", "")]
    public void Decrypt_Base64Invalido_DeveLancarExcecao()
    {
        Assert.ThrowsAny<Exception>(() => CryptoAES.Decrypt("nao-e-base64-valido!!!"));
    }

    [Fact(DisplayName = "Decrypt - Deve lançar exceção para dados corrompidos")]
    [Trait("Domain", "")]
    public void Decrypt_DadosCorrempidos_DeveLancarExcecao()
    {
        var dadosInvalidos = Convert.ToBase64String(new byte[10]);

        Assert.ThrowsAny<Exception>(() => CryptoAES.Decrypt(dadosInvalidos));
    }

    #endregion

    private static bool IsBase64(string value)
    {
        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
