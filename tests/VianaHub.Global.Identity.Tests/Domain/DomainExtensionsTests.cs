using VianaHub.Global.Identity.Domain.Helpers;
using System.ComponentModel;
using System.Text;

namespace VianaHub.Global.Identity.Tests.Domain;

public class DomainExtensionsTests
{
    #region SanitizeCsvValue

    [Fact(DisplayName = "SanitizeCsvValue - Deve retornar string vazia quando valor é nulo ou vazio")]
    [Trait("Domain", "")]
    public void SanitizeCsvValue_NullOuVazio_DeveRetornarMesmoValor()
    {
        Assert.Equal(string.Empty, string.Empty.SanitizeCsvValue());
        Assert.Null(((string)null!).SanitizeCsvValue());
    }

    [Theory(DisplayName = "SanitizeCsvValue - Deve remover caracteres perigosos do início")]
    [Trait("Domain", "")]
    [InlineData("=cmd", "cmd")]
    [InlineData("+cmd", "cmd")]
    [InlineData("-cmd", "cmd")]
    [InlineData("@cmd", "cmd")]
    public void SanitizeCsvValue_CaracterePerigoso_DeveRemover(string input, string expected)
    {
        var result = input.SanitizeCsvValue();
        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "SanitizeCsvValue - Deve retornar valor normal sem alterações")]
    [Trait("Domain", "")]
    public void SanitizeCsvValue_ValorNormal_DeveRetornarSemAlteracoes()
    {
        var result = "texto seguro".SanitizeCsvValue();
        Assert.Equal("texto seguro", result);
    }

    [Fact(DisplayName = "SanitizeCsvValue - Deve truncar valor maior que 5000 caracteres")]
    [Trait("Domain", "")]
    public void SanitizeCsvValue_ValorMuitoLongo_DeveTruncar()
    {
        var longValue = new string('a', 6000);
        var result = longValue.SanitizeCsvValue();
        Assert.Equal(5000, result.Length);
    }

    #endregion

    #region SanitizeCsvInput

    [Fact(DisplayName = "SanitizeCsvInput - Deve retornar valor quando nulo ou espaço em branco")]
    [Trait("Domain", "")]
    public void SanitizeCsvInput_NullOuEspacos_DeveRetornarMesmoValor()
    {
        Assert.Equal("   ", "   ".SanitizeCsvInput());
    }

    [Fact(DisplayName = "SanitizeCsvInput - Deve remover null bytes")]
    [Trait("Domain", "")]
    public void SanitizeCsvInput_ComNullByte_DeveRemover()
    {
        var result = "texto\0valido".SanitizeCsvInput();
        Assert.Equal("textovalido", result);
    }

    [Fact(DisplayName = "SanitizeCsvInput - Deve normalizar espaços múltiplos")]
    [Trait("Domain", "")]
    public void SanitizeCsvInput_EspacosMultiplos_DeveNormalizar()
    {
        var result = "texto   com   espacos".SanitizeCsvInput();
        Assert.Equal("texto com espacos", result);
    }

    #endregion

    #region IsSafeCsvValue

    [Fact(DisplayName = "IsSafeCsvValue - Deve retornar true quando valor é seguro")]
    [Trait("Domain", "")]
    public void IsSafeCsvValue_ValorSeguro_DeveRetornarTrue()
    {
        Assert.True("texto normal".IsSafeCsvValue());
    }

    [Fact(DisplayName = "IsSafeCsvValue - Deve retornar true quando valor é vazio")]
    [Trait("Domain", "")]
    public void IsSafeCsvValue_ValorVazio_DeveRetornarTrue()
    {
        Assert.True(string.Empty.IsSafeCsvValue());
    }

    [Theory(DisplayName = "IsSafeCsvValue - Deve retornar false para padrões perigosos")]
    [Trait("Domain", "")]
    [InlineData("=formula")]
    [InlineData("+formula")]
    [InlineData("javascript:alert(1)")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("cmd|powershell")]
    public void IsSafeCsvValue_ValorPerigoso_DeveRetornarFalse(string input)
    {
        Assert.False(input.IsSafeCsvValue());
    }

    #endregion

    #region IsValidCsvFileSize

    [Fact(DisplayName = "IsValidCsvFileSize - Deve retornar true para tamanho válido")]
    [Trait("Domain", "")]
    public void IsValidCsvFileSize_TamanhoValido_DeveRetornarTrue()
    {
        Assert.True(((long)(5 * 1024 * 1024)).IsValidCsvFileSize());
    }

    [Fact(DisplayName = "IsValidCsvFileSize - Deve retornar false para tamanho zero")]
    [Trait("Domain", "")]
    public void IsValidCsvFileSize_TamanhoZero_DeveRetornarFalse()
    {
        Assert.False(((long)0).IsValidCsvFileSize());
    }

    [Fact(DisplayName = "IsValidCsvFileSize - Deve retornar false para tamanho acima do limite")]
    [Trait("Domain", "")]
    public void IsValidCsvFileSize_TamanhoAcimaLimite_DeveRetornarFalse()
    {
        Assert.False(((long)(11 * 1024 * 1024)).IsValidCsvFileSize());
    }

    #endregion

    #region HasValidCsvExtension

    [Fact(DisplayName = "HasValidCsvExtension - Deve retornar true para extensão .csv")]
    [Trait("Domain", "")]
    public void HasValidCsvExtension_ExtensaoCsv_DeveRetornarTrue()
    {
        Assert.True("arquivo.csv".HasValidCsvExtension());
    }

    [Fact(DisplayName = "HasValidCsvExtension - Deve retornar false para extensão inválida")]
    [Trait("Domain", "")]
    public void HasValidCsvExtension_ExtensaoInvalida_DeveRetornarFalse()
    {
        Assert.False("arquivo.exe".HasValidCsvExtension());
    }

    [Fact(DisplayName = "HasValidCsvExtension - Deve retornar false para nome vazio")]
    [Trait("Domain", "")]
    public void HasValidCsvExtension_NomeVazio_DeveRetornarFalse()
    {
        Assert.False(string.Empty.HasValidCsvExtension());
    }

    [Fact(DisplayName = "HasValidCsvExtension - Deve aceitar extensões customizadas")]
    [Trait("Domain", "")]
    public void HasValidCsvExtension_ExtensaoCustomizada_DeveRetornarTrue()
    {
        Assert.True("arquivo.txt".HasValidCsvExtension(".txt", ".csv"));
    }

    #endregion

    #region IsSafeCsvFileName

    [Fact(DisplayName = "IsSafeCsvFileName - Deve retornar true para nome seguro")]
    [Trait("Domain", "")]
    public void IsSafeCsvFileName_NomeSeguro_DeveRetornarTrue()
    {
        Assert.True("arquivo.csv".IsSafeCsvFileName());
    }

    [Fact(DisplayName = "IsSafeCsvFileName - Deve retornar false para nome com path traversal")]
    [Trait("Domain", "")]
    public void IsSafeCsvFileName_PathTraversal_DeveRetornarFalse()
    {
        Assert.False("../../etc/passwd".IsSafeCsvFileName());
    }

    [Theory(DisplayName = "IsSafeCsvFileName - Deve retornar false para nomes com caracteres perigosos")]
    [Trait("Domain", "")]
    [InlineData("arquivo<>.csv")]
    [InlineData("arquivo?.csv")]
    [InlineData("arquivo*.csv")]
    public void IsSafeCsvFileName_CaracteresPerigosos_DeveRetornarFalse(string fileName)
    {
        Assert.False(fileName.IsSafeCsvFileName());
    }

    [Fact(DisplayName = "IsSafeCsvFileName - Deve retornar false para nome vazio")]
    [Trait("Domain", "")]
    public void IsSafeCsvFileName_NomeVazio_DeveRetornarFalse()
    {
        Assert.False(string.Empty.IsSafeCsvFileName());
    }

    #endregion

    #region NormalizeUtf8

    [Fact(DisplayName = "NormalizeUtf8 - Deve retornar valor quando nulo ou vazio")]
    [Trait("Domain", "")]
    public void NormalizeUtf8_NullOuVazio_DeveRetornarMesmoValor()
    {
        Assert.Null(((string)null!).NormalizeUtf8());
        Assert.Equal(string.Empty, string.Empty.NormalizeUtf8());
    }

    [Fact(DisplayName = "NormalizeUtf8 - Deve remover BOM")]
    [Trait("Domain", "")]
    public void NormalizeUtf8_ComBom_DeveRemover()
    {
        var valueWithBom = "\uFEFFtexto";
        var result = valueWithBom.NormalizeUtf8();
        Assert.Equal("texto", result);
    }

    [Fact(DisplayName = "NormalizeUtf8 - Deve retornar texto normal sem alterações")]
    [Trait("Domain", "")]
    public void NormalizeUtf8_TextoNormal_DeveRetornarSemAlteracoes()
    {
        var result = "texto normal".NormalizeUtf8();
        Assert.Equal("texto normal", result);
    }

    #endregion

    #region IsValidCsvRowCount

    [Fact(DisplayName = "IsValidCsvRowCount - Deve retornar true para contagem válida")]
    [Trait("Domain", "")]
    public void IsValidCsvRowCount_ContagemValida_DeveRetornarTrue()
    {
        Assert.True(100.IsValidCsvRowCount());
    }

    [Fact(DisplayName = "IsValidCsvRowCount - Deve retornar false para contagem zero")]
    [Trait("Domain", "")]
    public void IsValidCsvRowCount_ContagemZero_DeveRetornarFalse()
    {
        Assert.False(0.IsValidCsvRowCount());
    }

    [Fact(DisplayName = "IsValidCsvRowCount - Deve retornar false para contagem acima do limite")]
    [Trait("Domain", "")]
    public void IsValidCsvRowCount_AcimaDoLimite_DeveRetornarFalse()
    {
        Assert.False(10001.IsValidCsvRowCount());
    }

    #endregion

    #region IsValidNonEmptyGuid

    [Fact(DisplayName = "IsValidNonEmptyGuid - Deve retornar true para GUID válido")]
    [Trait("Domain", "")]
    public void IsValidNonEmptyGuid_GuidValido_DeveRetornarTrue()
    {
        var guid = Guid.NewGuid().ToString();
        Assert.True(guid.IsValidNonEmptyGuid(out var parsed));
        Assert.NotEqual(Guid.Empty, parsed);
    }

    [Fact(DisplayName = "IsValidNonEmptyGuid - Deve retornar false para GUID vazio")]
    [Trait("Domain", "")]
    public void IsValidNonEmptyGuid_GuidVazio_DeveRetornarFalse()
    {
        Assert.False(Guid.Empty.ToString().IsValidNonEmptyGuid(out _));
    }

    [Fact(DisplayName = "IsValidNonEmptyGuid - Deve retornar false para string inválida")]
    [Trait("Domain", "")]
    public void IsValidNonEmptyGuid_StringInvalida_DeveRetornarFalse()
    {
        Assert.False("nao-e-guid".IsValidNonEmptyGuid(out _));
    }

    [Fact(DisplayName = "IsValidNonEmptyGuid - Deve retornar false para string vazia")]
    [Trait("Domain", "")]
    public void IsValidNonEmptyGuid_StringVazia_DeveRetornarFalse()
    {
        Assert.False(string.Empty.IsValidNonEmptyGuid(out _));
    }

    #endregion

    #region SanitizeCsvFields

    [Fact(DisplayName = "SanitizeCsvFields - Deve sanitizar todos os campos do dicionário")]
    [Trait("Domain", "")]
    public void SanitizeCsvFields_ComCamposPerigosos_DeveSanitizar()
    {
        var fields = new Dictionary<string, string>
        {
            { "nome", "=fórmula" },
            { "descricao", "texto normal" }
        };

        var result = fields.SanitizeCsvFields();

        Assert.Equal("fórmula", result["nome"]);
        Assert.Equal("texto normal", result["descricao"]);
    }

    [Fact(DisplayName = "SanitizeCsvFields - Deve retornar mesmo dicionário quando vazio")]
    [Trait("Domain", "")]
    public void SanitizeCsvFields_DicionarioVazio_DeveRetornarMesmo()
    {
        var fields = new Dictionary<string, string>();
        var result = fields.SanitizeCsvFields();
        Assert.Empty(result);
    }

    [Fact(DisplayName = "SanitizeCsvFields - Deve retornar null quando dicionário é null")]
    [Trait("Domain", "")]
    public void SanitizeCsvFields_DicionarioNull_DeveRetornarNull()
    {
        Dictionary<string, string> fields = null!;
        var result = fields.SanitizeCsvFields();
        Assert.Null(result);
    }

    #endregion

    #region NormalizeWhitespace

    [Fact(DisplayName = "NormalizeWhitespace - Deve normalizar espaços múltiplos")]
    [Trait("Domain", "")]
    public void NormalizeWhitespace_EspacosMultiplos_DeveNormalizar()
    {
        var result = "texto   com   espacos".NormalizeWhitespace();
        Assert.Equal("texto com espacos", result);
    }

    [Fact(DisplayName = "NormalizeWhitespace - Deve retornar valor quando nulo ou espaço em branco")]
    [Trait("Domain", "")]
    public void NormalizeWhitespace_NullOuBranco_DeveRetornarMesmoValor()
    {
        Assert.Null(((string)null!).NormalizeWhitespace());
        Assert.Equal("   ", "   ".NormalizeWhitespace());
    }

    #endregion

    #region ContainsOnlySafeCharacters

    [Fact(DisplayName = "ContainsOnlySafeCharacters - Deve retornar true para texto seguro com especiais")]
    [Trait("Domain", "")]
    public void ContainsOnlySafeCharacters_TextoSeguro_DeveRetornarTrue()
    {
        Assert.True("texto seguro com pontuação.".ContainsOnlySafeCharacters());
    }

    [Fact(DisplayName = "ContainsOnlySafeCharacters - Deve retornar false para texto com caracteres perigosos")]
    [Trait("Domain", "")]
    public void ContainsOnlySafeCharacters_CaracteresPerigosos_DeveRetornarFalse()
    {
        Assert.False("<script>".ContainsOnlySafeCharacters());
    }

    [Fact(DisplayName = "ContainsOnlySafeCharacters - Deve retornar true para string vazia")]
    [Trait("Domain", "")]
    public void ContainsOnlySafeCharacters_StringVazia_DeveRetornarTrue()
    {
        Assert.True(string.Empty.ContainsOnlySafeCharacters());
    }

    [Fact(DisplayName = "ContainsOnlySafeCharacters - Deve retornar false para texto com especiais quando não permitido")]
    [Trait("Domain", "")]
    public void ContainsOnlySafeCharacters_SemEspeciaisPermitidos_DeveRetornarFalse()
    {
        Assert.False("texto!".ContainsOnlySafeCharacters(allowSpecialChars: false));
    }

    #endregion

    #region GenerateCode

    [Fact(DisplayName = "GenerateCode - Deve gerar código com primeira e última palavra")]
    [Trait("Domain", "")]
    public void GenerateCode_NomeComposto_DeveGerarCodigo()
    {
        var result = "Identity Api Process".GenerateCode();
        Assert.Equal("identity-process", result);
    }

    [Fact(DisplayName = "GenerateCode - Deve retornar palavra única em minúsculo")]
    [Trait("Domain", "")]
    public void GenerateCode_PalavraUnica_DeveRetornarEmMinusculo()
    {
        var result = "Identity".GenerateCode();
        Assert.Equal("identity", result);
    }

    [Fact(DisplayName = "GenerateCode - Deve retornar vazio para string nula ou vazia")]
    [Trait("Domain", "")]
    public void GenerateCode_NullOuVazio_DeveRetornarVazio()
    {
        Assert.Equal(string.Empty, string.Empty.GenerateCode());
        Assert.Equal(string.Empty, "   ".GenerateCode());
    }

    #endregion

    #region CreateShortId

    [Fact(DisplayName = "CreateShortId - Deve gerar ShortId com formato correto")]
    [Trait("Domain", "")]
    public async Task CreateShortId_Sucesso_DeveGerarComFormato()
    {
        var result = await DomainExtensions.CreateShortId();

        Assert.NotNull(result);
        Assert.Equal(16, result.Length);
    }

    #endregion

    #region FirstCharToUpper

    [Fact(DisplayName = "FirstCharToUpper - Deve capitalizar a primeira letra de cada palavra")]
    [Trait("Domain", "")]
    public void FirstCharToUpper_TextoNormal_DeveCapitalizar()
    {
        var result = "identidade de acesso".FirstCharToUpper();
        Assert.Equal("Identidade de Acesso", result);
    }

    [Fact(DisplayName = "FirstCharToUpper - Deve retornar null para string nula ou vazia")]
    [Trait("Domain", "")]
    public void FirstCharToUpper_NullOuVazio_DeveRetornarNull()
    {
        Assert.Null(((string)null!).FirstCharToUpper());
        Assert.Null(string.Empty.FirstCharToUpper());
    }

    #endregion

    #region IsNumeric / IsInt / IsGuid

    [Fact(DisplayName = "IsNumeric - Deve retornar true para string numérica")]
    [Trait("Domain", "")]
    public async Task IsNumeric_StringNumerica_DeveRetornarTrue()
    {
        Assert.True(await "123.45".IsNumeric());
    }

    [Fact(DisplayName = "IsNumeric - Deve retornar false para string não numérica")]
    [Trait("Domain", "")]
    public async Task IsNumeric_StringNaoNumerica_DeveRetornarFalse()
    {
        Assert.False(await "abc".IsNumeric());
    }

    [Fact(DisplayName = "IsInt - Deve retornar true para string inteira")]
    [Trait("Domain", "")]
    public async Task IsInt_StringInteira_DeveRetornarTrue()
    {
        Assert.True(await "42".IsInt());
    }

    [Fact(DisplayName = "IsInt - Deve retornar false para string não inteira")]
    [Trait("Domain", "")]
    public async Task IsInt_StringNaoInteira_DeveRetornarFalse()
    {
        Assert.False(await "3.14".IsInt());
    }

    [Fact(DisplayName = "IsGuid - Deve retornar true para GUID válido")]
    [Trait("Domain", "")]
    public async Task IsGuid_GuidValido_DeveRetornarTrue()
    {
        Assert.True(await Guid.NewGuid().ToString("D").IsGuid());
    }

    [Fact(DisplayName = "IsGuid - Deve retornar false para string inválida")]
    [Trait("Domain", "")]
    public async Task IsGuid_StringInvalida_DeveRetornarFalse()
    {
        Assert.False(await "nao-e-guid".IsGuid());
    }

    #endregion

    #region IsShortId

    [Fact(DisplayName = "IsShortId - Deve retornar true para ShortId válido")]
    [Trait("Domain", "")]
    public async Task IsShortId_ValorValido_DeveRetornarTrue()
    {
        var shortId = await DomainExtensions.CreateShortId();
        Assert.True(await shortId.IsShortId());
    }

    [Fact(DisplayName = "IsShortId - Deve retornar false para ShortId inválido")]
    [Trait("Domain", "")]
    public async Task IsShortId_ValorInvalido_DeveRetornarFalse()
    {
        Assert.False(await "invalido".IsShortId());
    }

    #endregion

    #region IsEmail

    [Fact(DisplayName = "IsEmail - Deve retornar true para e-mail válido")]
    [Trait("Domain", "")]
    public async Task IsEmail_EmailValido_DeveRetornarTrue()
    {
        Assert.True(await "usuario@dominio.com".IsEmail());
    }

    [Fact(DisplayName = "IsEmail - Deve retornar false para e-mail inválido")]
    [Trait("Domain", "")]
    public async Task IsEmail_EmailInvalido_DeveRetornarFalse()
    {
        Assert.False(await "nao-e-email".IsEmail());
    }

    #endregion

    #region ToPhoneFormatPortugal

    [Fact(DisplayName = "ToPhoneFormatPortugal - Deve formatar número com 9 dígitos")]
    [Trait("Domain", "")]
    public void ToPhoneFormatPortugal_NumerValido_DeveFormatar()
    {
        var result = "912345678".ToPhoneFormatPortugal();
        Assert.Equal("912 345 678", result);
    }

    [Fact(DisplayName = "ToPhoneFormatPortugal - Deve retornar original para número inválido")]
    [Trait("Domain", "")]
    public void ToPhoneFormatPortugal_NumeroInvalido_DeveRetornarOriginal()
    {
        var result = "12345".ToPhoneFormatPortugal();
        Assert.Equal("12345", result);
    }

    [Fact(DisplayName = "ToPhoneFormatPortugal - Deve retornar vazio para string vazia")]
    [Trait("Domain", "")]
    public void ToPhoneFormatPortugal_StringVazia_DeveRetornarVazio()
    {
        var result = string.Empty.ToPhoneFormatPortugal();
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region LastTimeDay / StringShortDate

    [Fact(DisplayName = "LastTimeDay - Deve retornar último horário do dia")]
    [Trait("Domain", "")]
    public void LastTimeDay_DataValida_DeveRetornarUltimoHorario()
    {
        var data = new DateTime(2024, 6, 15);
        var result = data.LastTimeDay();
        Assert.Equal(new DateTime(2024, 6, 15, 23, 59, 59), result);
    }

    [Fact(DisplayName = "StringShortDate - Deve formatar data corretamente")]
    [Trait("Domain", "")]
    public void StringShortDate_DataValida_DeveFormatar()
    {
        var data = new DateTime(2024, 6, 15);
        var result = data.StringShortDate();
        Assert.Contains("15", result);
        Assert.Contains("06", result);
        Assert.Contains("2024", result);
    }

    #endregion

    #region RemoveAccentsAndSpecialCharacters

    [Fact(DisplayName = "RemoveAccentsAndSpecialCharacters - Deve remover acentos")]
    [Trait("Domain", "")]
    public void RemoveAccentsAndSpecialCharacters_ComAcentos_DeveRemover()
    {
        var result = "ação".RemoveAccentsAndSpecialCharacters();
        Assert.Equal("acao", result);
    }

    [Fact(DisplayName = "RemoveAccentsAndSpecialCharacters - Deve remover caracteres especiais")]
    [Trait("Domain", "")]
    public void RemoveAccentsAndSpecialCharacters_ComEspeciais_DeveRemover()
    {
        var result = "texto@especial!".RemoveAccentsAndSpecialCharacters();
        Assert.DoesNotContain("@", result);
        Assert.DoesNotContain("!", result);
    }

    #endregion

    #region DateMajorEqualNow

    [Fact(DisplayName = "DateMajorEqualNow - Deve retornar true para data futura")]
    [Trait("Domain", "")]
    public void DateMajorEqualNow_DataFutura_DeveRetornarTrue()
    {
        Assert.True(DateTime.Now.AddDays(1).DateMajorEqualNow());
    }

    [Fact(DisplayName = "DateMajorEqualNow - Deve retornar false para data passada")]
    [Trait("Domain", "")]
    public void DateMajorEqualNow_DataPassada_DeveRetornarFalse()
    {
        Assert.False(DateTime.Now.AddDays(-1).DateMajorEqualNow());
    }

    #endregion

    #region NotEmpty

    [Fact(DisplayName = "NotEmpty - Deve retornar true para string não nula")]
    [Trait("Domain", "")]
    public void NotEmpty_StringNaoNula_DeveRetornarTrue()
    {
        Assert.True("valor".NotEmpty());
    }

    [Fact(DisplayName = "NotEmpty - Deve retornar false para string nula")]
    [Trait("Domain", "")]
    public void NotEmpty_StringNula_DeveRetornarFalse()
    {
        Assert.False(((string)null!).NotEmpty());
    }

    #endregion

    #region ToByteArray / ToBase64

    [Fact(DisplayName = "ToByteArray - Deve converter Base64 para byte array")]
    [Trait("Domain", "")]
    public void ToByteArray_Base64Valido_DeveConverter()
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("teste"));
        var result = base64.ToByteArray();
        Assert.Equal("teste", Encoding.UTF8.GetString(result));
    }

    [Fact(DisplayName = "ToByteArray - Deve lançar exceção para string nula ou vazia")]
    [Trait("Domain", "")]
    public void ToByteArray_StringVazia_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() => string.Empty.ToByteArray());
    }

    [Fact(DisplayName = "ToBase64 - Deve converter byte array para Base64")]
    [Trait("Domain", "")]
    public void ToBase64_ByteArrayValido_DeveConverter()
    {
        var bytes = Encoding.UTF8.GetBytes("teste");
        var result = bytes.ToBase64();
        Assert.Equal(Convert.ToBase64String(bytes), result);
    }

    [Fact(DisplayName = "ToBase64 - Deve lançar exceção para array nulo ou vazio")]
    [Trait("Domain", "")]
    public void ToBase64_ArrayVazio_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() => Array.Empty<byte>().ToBase64());
    }

    #endregion

    #region GenerateClientSecret / HashClientSecret / VerifyClientSecret

    [Fact(DisplayName = "GenerateClientSecret - Deve gerar secret com tamanho correto")]
    [Trait("Domain", "")]
    public void GenerateClientSecret_Padrao_DeveGerarComTamanhoCorreto()
    {
        var result = DomainExtensions.GenerateClientSecret();
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact(DisplayName = "GenerateClientSecret - Deve gerar secrets únicos a cada chamada")]
    [Trait("Domain", "")]
    public void GenerateClientSecret_DuasChamadas_DevemSerDiferentes()
    {
        var secret1 = DomainExtensions.GenerateClientSecret();
        var secret2 = DomainExtensions.GenerateClientSecret();
        Assert.NotEqual(secret1, secret2);
    }

    [Fact(DisplayName = "HashClientSecret - Deve gerar hash válido para secret não vazio")]
    [Trait("Domain", "")]
    public void HashClientSecret_SecretValido_DeveGerarHash()
    {
        var hash = DomainExtensions.HashClientSecret("meu-secret");
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact(DisplayName = "HashClientSecret - Deve lançar exceção para secret vazio")]
    [Trait("Domain", "")]
    public void HashClientSecret_SecretVazio_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() => DomainExtensions.HashClientSecret(string.Empty));
    }

    [Fact(DisplayName = "VerifyClientSecret - Deve retornar true para secret correto")]
    [Trait("Domain", "")]
    public void VerifyClientSecret_SecretCorreto_DeveRetornarTrue()
    {
        var secret = "meu-secret-seguro";
        var hash = DomainExtensions.HashClientSecret(secret);
        Assert.True(DomainExtensions.VerifyClientSecret(hash, secret));
    }

    [Fact(DisplayName = "VerifyClientSecret - Deve retornar false para secret incorreto")]
    [Trait("Domain", "")]
    public void VerifyClientSecret_SecretIncorreto_DeveRetornarFalse()
    {
        var hash = DomainExtensions.HashClientSecret("secret-correto");
        Assert.False(DomainExtensions.VerifyClientSecret(hash, "secret-errado"));
    }

    [Fact(DisplayName = "VerifyClientSecret - Deve retornar false para hash vazio")]
    [Trait("Domain", "")]
    public void VerifyClientSecret_HashVazio_DeveRetornarFalse()
    {
        Assert.False(DomainExtensions.VerifyClientSecret(string.Empty, "secret"));
    }

    #endregion

    #region ValidateHexadecimalSequence

    [Fact(DisplayName = "ValidateHexadecimalSequence - Deve retornar true para sequência gerada")]
    [Trait("Domain", "")]
    public void ValidateHexadecimalSequence_SequenciaGerada_DeveRetornarTrue()
    {
        var seq = DomainExtensions.GenerateHexadecimalSequence();
        Assert.True(DomainExtensions.ValidateHexadecimalSequence(seq));
    }

    [Fact(DisplayName = "ValidateHexadecimalSequence - Deve retornar false para sequência inválida")]
    [Trait("Domain", "")]
    public void ValidateHexadecimalSequence_SequenciaInvalida_DeveRetornarFalse()
    {
        // Sequência hexadecimal de 10 chars mas com dígitos verificadores incorretos
        Assert.False(DomainExtensions.ValidateHexadecimalSequence("AABBCCDD00"));
    }

    [Fact(DisplayName = "ValidateHexadecimalSequence - Deve retornar false para tamanho errado")]
    [Trait("Domain", "")]
    public void ValidateHexadecimalSequence_TamanhoErrado_DeveRetornarFalse()
    {
        Assert.False(DomainExtensions.ValidateHexadecimalSequence("ABCD"));
    }

    #endregion

    #region GetDescription / GetEnumByDescription

    private enum StatusTeste
    {
        [Description("Ativo")]
        Ativo = 1,
        [Description("Inativo")]
        Inativo = 2
    }

    [Fact(DisplayName = "GetDescription - Deve retornar descrição do enum")]
    [Trait("Domain", "")]
    public void GetDescription_EnumComDescricao_DeveRetornar()
    {
        var result = StatusTeste.Ativo.GetDescription();
        Assert.Equal("Ativo", result);
    }

    [Fact(DisplayName = "GetDescription - Deve retornar string vazia para enum nulo")]
    [Trait("Domain", "")]
    public void GetDescription_EnumNulo_DeveRetornarVazio()
    {
        Enum value = null!;
        Assert.Equal(string.Empty, value.GetDescription());
    }

    [Fact(DisplayName = "GetDescription - Deve retornar descrição por valor inteiro")]
    [Trait("Domain", "")]
    public void GetDescription_PorValorInteiro_DeveRetornar()
    {
        var result = 1.GetDescription<StatusTeste>();
        Assert.Equal("Ativo", result);
    }

    [Fact(DisplayName = "GetEnumByDescription - Deve retornar enum pela descrição")]
    [Trait("Domain", "")]
    public void GetEnumByDescription_DescricaoValida_DeveRetornarEnum()
    {
        var result = DomainExtensions.GetEnumByDescription<StatusTeste>("Ativo");
        Assert.Equal(StatusTeste.Ativo, result);
    }

    [Fact(DisplayName = "GetEnumByDescription - Deve lançar exceção para descrição inválida")]
    [Trait("Domain", "")]
    public void GetEnumByDescription_DescricaoInvalida_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() => DomainExtensions.GetEnumByDescription<StatusTeste>("Inexistente"));
    }

    #endregion

    #region IsValidUtf8Encoding

    [Fact(DisplayName = "IsValidUtf8Encoding - Deve retornar true para stream UTF-8 válido")]
    [Trait("Domain", "")]
    public void IsValidUtf8Encoding_StreamValido_DeveRetornarTrue()
    {
        var bytes = Encoding.UTF8.GetBytes("texto válido em UTF-8");
        using var stream = new MemoryStream(bytes);
        Assert.True(stream.IsValidUtf8Encoding());
    }

    [Fact(DisplayName = "IsValidUtf8Encoding - Deve retornar false para stream nulo")]
    [Trait("Domain", "")]
    public void IsValidUtf8Encoding_StreamNulo_DeveRetornarFalse()
    {
        Stream stream = null!;
        Assert.False(stream.IsValidUtf8Encoding());
    }

    #endregion

    #region CreateUtf8StreamReader

    [Fact(DisplayName = "CreateUtf8StreamReader - Deve criar StreamReader para stream válido")]
    [Trait("Domain", "")]
    public void CreateUtf8StreamReader_StreamValido_DeveCriar()
    {
        var bytes = Encoding.UTF8.GetBytes("conteúdo");
        using var stream = new MemoryStream(bytes);
        using var reader = stream.CreateUtf8StreamReader();
        Assert.NotNull(reader);
        Assert.Equal("conteúdo", reader.ReadToEnd());
    }

    [Fact(DisplayName = "CreateUtf8StreamReader - Deve lançar exceção para stream nulo")]
    [Trait("Domain", "")]
    public void CreateUtf8StreamReader_StreamNulo_DeveLancarExcecao()
    {
        Stream stream = null!;
        Assert.Throws<ArgumentNullException>(() => stream.CreateUtf8StreamReader());
    }

    #endregion
}
