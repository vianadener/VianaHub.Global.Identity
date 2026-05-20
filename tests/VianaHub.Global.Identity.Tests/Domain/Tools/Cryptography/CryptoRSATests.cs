using VianaHub.Global.Identity.Domain.Tools.Cryptography;

namespace VianaHub.Global.Identity.Tests.Domain.Tools.Cryptography;

public class CryptoRSATests
{
    private const string EncryptionKey = "chave-de-criptografia-32bytes!!";

    #region GenerateKeyPair

    [Fact(DisplayName = "GenerateKeyPair - Deve retornar par de chaves PEM não vazias")]
    [Trait("Domain", "")]
    public void GenerateKeyPair_TamanhoPadrao_DeveRetornarChavesPemValidas()
    {
        var (publicKey, privateKey) = CryptoRSA.GenerateKeyPair();

        Assert.NotNull(publicKey);
        Assert.NotNull(privateKey);
        Assert.NotEmpty(publicKey);
        Assert.NotEmpty(privateKey);
    }

    [Fact(DisplayName = "GenerateKeyPair - Chave pública deve conter cabeçalho PEM correto")]
    [Trait("Domain", "")]
    public void GenerateKeyPair_ChavePublica_DeveConterCabecalhoPem()
    {
        var (publicKey, _) = CryptoRSA.GenerateKeyPair();

        Assert.Contains("-----BEGIN PUBLIC KEY-----", publicKey);
        Assert.Contains("-----END PUBLIC KEY-----", publicKey);
    }

    [Fact(DisplayName = "GenerateKeyPair - Chave privada deve conter cabeçalho PEM correto")]
    [Trait("Domain", "")]
    public void GenerateKeyPair_ChavePrivada_DeveConterCabecalhoPem()
    {
        var (_, privateKey) = CryptoRSA.GenerateKeyPair();

        Assert.Contains("-----BEGIN PRIVATE KEY-----", privateKey);
        Assert.Contains("-----END PRIVATE KEY-----", privateKey);
    }

    [Fact(DisplayName = "GenerateKeyPair - Deve gerar pares de chaves únicas a cada chamada")]
    [Trait("Domain", "")]
    public void GenerateKeyPair_MultiplasChamadas_DeveGerarChavesDiferentes()
    {
        var (publicKey1, privateKey1) = CryptoRSA.GenerateKeyPair();
        var (publicKey2, privateKey2) = CryptoRSA.GenerateKeyPair();

        Assert.NotEqual(publicKey1, publicKey2);
        Assert.NotEqual(privateKey1, privateKey2);
    }

    [Theory(DisplayName = "GenerateKeyPair - Deve suportar diferentes tamanhos de chave")]
    [Trait("Domain", "")]
    [InlineData(2048)]
    [InlineData(3072)]
    [InlineData(4096)]
    public void GenerateKeyPair_TamanhosValidos_DeveGerarChaves(int keySize)
    {
        var (publicKey, privateKey) = CryptoRSA.GenerateKeyPair(keySize);

        Assert.NotEmpty(publicKey);
        Assert.NotEmpty(privateKey);
    }

    #endregion

    #region EncryptPrivateKey

    [Fact(DisplayName = "EncryptPrivateKey - Deve criptografar chave privada e retornar Base64")]
    [Trait("Domain", "")]
    public void EncryptPrivateKey_ChaveValida_DeveRetornarBase64()
    {
        var (_, privateKey) = CryptoRSA.GenerateKeyPair();

        var result = CryptoRSA.EncryptPrivateKey(privateKey, EncryptionKey);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(IsBase64(result));
    }

    [Fact(DisplayName = "EncryptPrivateKey - Deve lançar ArgumentException para chave privada nula")]
    [Trait("Domain", "")]
    public void EncryptPrivateKey_ChavePrivadaNula_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CryptoRSA.EncryptPrivateKey(null!, EncryptionKey));
    }

    [Fact(DisplayName = "EncryptPrivateKey - Deve lançar ArgumentException para chave privada vazia")]
    [Trait("Domain", "")]
    public void EncryptPrivateKey_ChavePrivadaVazia_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CryptoRSA.EncryptPrivateKey(string.Empty, EncryptionKey));
    }

    [Fact(DisplayName = "EncryptPrivateKey - Deve lançar ArgumentException para chave de criptografia nula")]
    [Trait("Domain", "")]
    public void EncryptPrivateKey_ChaveCriptografiaNula_DeveLancarArgumentException()
    {
        var (_, privateKey) = CryptoRSA.GenerateKeyPair();

        Assert.Throws<ArgumentException>(() => CryptoRSA.EncryptPrivateKey(privateKey, null!));
    }

    [Fact(DisplayName = "EncryptPrivateKey - Deve lançar ArgumentException para chave de criptografia vazia")]
    [Trait("Domain", "")]
    public void EncryptPrivateKey_ChaveCriptografiaVazia_DeveLancarArgumentException()
    {
        var (_, privateKey) = CryptoRSA.GenerateKeyPair();

        Assert.Throws<ArgumentException>(() => CryptoRSA.EncryptPrivateKey(privateKey, string.Empty));
    }

    #endregion

    #region DecryptPrivateKey

    [Fact(DisplayName = "DecryptPrivateKey - Deve retornar chave privada original após criptografar e descriptografar")]
    [Trait("Domain", "")]
    public void DecryptPrivateKey_ChaveCriptografada_DeveRetornarChaveOriginal()
    {
        var (_, privateKey) = CryptoRSA.GenerateKeyPair();
        var encrypted = CryptoRSA.EncryptPrivateKey(privateKey, EncryptionKey);

        var result = CryptoRSA.DecryptPrivateKey(encrypted, EncryptionKey);

        Assert.Equal(privateKey, result);
    }

    [Fact(DisplayName = "DecryptPrivateKey - Deve lançar ArgumentException para entrada nula")]
    [Trait("Domain", "")]
    public void DecryptPrivateKey_EntradaNula_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CryptoRSA.DecryptPrivateKey(null!, EncryptionKey));
    }

    [Fact(DisplayName = "DecryptPrivateKey - Deve lançar ArgumentException para entrada vazia")]
    [Trait("Domain", "")]
    public void DecryptPrivateKey_EntradaVazia_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CryptoRSA.DecryptPrivateKey(string.Empty, EncryptionKey));
    }

    [Fact(DisplayName = "DecryptPrivateKey - Deve lançar ArgumentException para chave de criptografia nula")]
    [Trait("Domain", "")]
    public void DecryptPrivateKey_ChaveCriptografiaNula_DeveLancarArgumentException()
    {
        var (_, privateKey) = CryptoRSA.GenerateKeyPair();
        var encrypted = CryptoRSA.EncryptPrivateKey(privateKey, EncryptionKey);

        Assert.Throws<ArgumentException>(() => CryptoRSA.DecryptPrivateKey(encrypted, null!));
    }

    [Fact(DisplayName = "DecryptPrivateKey - Deve lançar exceção para dados corrompidos")]
    [Trait("Domain", "")]
    public void DecryptPrivateKey_DadosCorrempidos_DeveLancarExcecao()
    {
        var dadosInvalidos = Convert.ToBase64String(new byte[32]);

        Assert.ThrowsAny<Exception>(() => CryptoRSA.DecryptPrivateKey(dadosInvalidos, EncryptionKey));
    }

    #endregion

    #region ExtractBytesFromPem

    [Fact(DisplayName = "ExtractBytesFromPem - Deve extrair bytes de PEM válido")]
    [Trait("Domain", "")]
    public void ExtractBytesFromPem_PemValido_DeveRetornarBytes()
    {
        var (publicKey, _) = CryptoRSA.GenerateKeyPair();

        var result = CryptoRSA.ExtractBytesFromPem(publicKey, "PUBLIC KEY");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact(DisplayName = "ExtractBytesFromPem - Deve lançar ArgumentException para PEM com formato inválido")]
    [Trait("Domain", "")]
    public void ExtractBytesFromPem_FormatoInvalido_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CryptoRSA.ExtractBytesFromPem("pem-invalido", "PUBLIC KEY"));
    }

    [Fact(DisplayName = "ExtractBytesFromPem - Deve lançar ArgumentException para label incorreta")]
    [Trait("Domain", "")]
    public void ExtractBytesFromPem_LabelIncorreta_DeveLancarArgumentException()
    {
        var (publicKey, _) = CryptoRSA.GenerateKeyPair();

        Assert.Throws<ArgumentException>(() => CryptoRSA.ExtractBytesFromPem(publicKey, "PRIVATE KEY"));
    }

    #endregion

    #region IsValidPem

    [Fact(DisplayName = "IsValidPem - Deve retornar true para PEM público válido")]
    [Trait("Domain", "")]
    public void IsValidPem_PemPublicoValido_DeveRetornarTrue()
    {
        var (publicKey, _) = CryptoRSA.GenerateKeyPair();

        var result = CryptoRSA.IsValidPem(publicKey, "PUBLIC KEY");

        Assert.True(result);
    }

    [Fact(DisplayName = "IsValidPem - Deve retornar true para PEM privado válido")]
    [Trait("Domain", "")]
    public void IsValidPem_PemPrivadoValido_DeveRetornarTrue()
    {
        var (_, privateKey) = CryptoRSA.GenerateKeyPair();

        var result = CryptoRSA.IsValidPem(privateKey, "PRIVATE KEY");

        Assert.True(result);
    }

    [Fact(DisplayName = "IsValidPem - Deve retornar false para PEM inválido")]
    [Trait("Domain", "")]
    public void IsValidPem_PemInvalido_DeveRetornarFalse()
    {
        var result = CryptoRSA.IsValidPem("pem-invalido", "PUBLIC KEY");

        Assert.False(result);
    }

    [Fact(DisplayName = "IsValidPem - Deve retornar false para label incorreta")]
    [Trait("Domain", "")]
    public void IsValidPem_LabelIncorreta_DeveRetornarFalse()
    {
        var (publicKey, _) = CryptoRSA.GenerateKeyPair();

        var result = CryptoRSA.IsValidPem(publicKey, "PRIVATE KEY");

        Assert.False(result);
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
