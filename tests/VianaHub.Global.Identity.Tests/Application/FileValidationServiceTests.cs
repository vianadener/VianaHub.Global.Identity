using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class FileValidationServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();

    public FileValidationServiceTests()
    {
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
    }

    private FileValidationService CreateSut() => new(_notifyMock.Object, _localizationMock.Object);

    private static Mock<IFormFile> BuildFileMock(byte[] content, string fileName = "dados.csv", string contentType = "text/csv")
    {
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns(fileName);
        fileMock.Setup(x => x.Length).Returns(content.Length);
        fileMock.Setup(x => x.ContentType).Returns(contentType);
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        return fileMock;
    }

    private static byte[] BuildUtf8CsvContent(string content = "Nome;Descricao\r\nTeste;Desc\r\n")
        => System.Text.Encoding.UTF8.GetBytes(content);

    #region ValidateFile - Arquivo nulo ou vazio

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando arquivo é nulo")]
    [Trait("Application", "")]
    public void ValidateFile_ArquivoNulo_DeveRetornarFalseENotificar()
    {
        var sut = CreateSut();
        var result = sut.ValidateFile(null);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando arquivo está vazio")]
    [Trait("Application", "")]
    public void ValidateFile_ArquivoVazio_DeveRetornarFalseENotificar()
    {
        var fileMock = BuildFileMock(Array.Empty<byte>());

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #endregion

    #region ValidateFile - Tamanho do arquivo

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando arquivo excede tamanho máximo")]
    [Trait("Application", "")]
    public void ValidateFile_TamanhoExcedido_DeveRetornarFalseENotificar()
    {
        // Simula arquivo maior que o limite (10 MB = 10.485.760 bytes)
        var oversizedContent = new byte[10_485_761];
        var fileMock = BuildFileMock(oversizedContent);

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #endregion

    #region ValidateFile - Nome do arquivo

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando nome do arquivo contém path traversal")]
    [Trait("Application", "")]
    public void ValidateFile_NomeComPathTraversal_DeveRetornarFalseENotificar()
    {
        var content = BuildUtf8CsvContent();
        var fileMock = BuildFileMock(content, "../../etc/passwd.csv");

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando nome do arquivo contém caracteres inválidos")]
    [Trait("Application", "")]
    public void ValidateFile_NomeComCaracteresInvalidos_DeveRetornarFalseENotificar()
    {
        var content = BuildUtf8CsvContent();
        var fileMock = BuildFileMock(content, "arq<ivo>.csv");

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #endregion

    #region ValidateFile - Extensão do arquivo

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando extensão não é CSV")]
    [Trait("Application", "")]
    public void ValidateFile_ExtensaoInvalida_DeveRetornarFalseENotificar()
    {
        var content = BuildUtf8CsvContent();
        var fileMock = BuildFileMock(content, "dados.xlsx");

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando extensão é txt")]
    [Trait("Application", "")]
    public void ValidateFile_ExtensaoTxt_DeveRetornarFalseENotificar()
    {
        var content = BuildUtf8CsvContent();
        var fileMock = BuildFileMock(content, "dados.txt");

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #endregion

    #region ValidateFile - Encoding

    [Fact(DisplayName = "ValidateFile - Deve retornar false e notificar quando encoding não é UTF-8")]
    [Trait("Application", "")]
    public void ValidateFile_EncodingInvalido_DeveRetornarFalseENotificar()
    {
        // Conteúdo em Latin-1 (ISO-8859-1) com caractere inválido no UTF-8
        var latin1Content = System.Text.Encoding.Latin1.GetBytes("Nome;Descricao\r\nTêste;Açaí\r\n");
        var fileMock = BuildFileMock(latin1Content, "dados.csv");

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #endregion

    #region ValidateFile - Sucesso

    [Fact(DisplayName = "ValidateFile - Deve retornar true quando arquivo CSV válido")]
    [Trait("Application", "")]
    public void ValidateFile_ArquivoValido_DeveRetornarTrue()
    {
        var content = BuildUtf8CsvContent("Nome;Descricao\r\nTeste;Descricao valida\r\n");
        var fileMock = BuildFileMock(content, "dados.csv");

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.True(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact(DisplayName = "ValidateFile - Deve retornar true quando arquivo CSV com extensão maiúscula")]
    [Trait("Application", "")]
    public void ValidateFile_ExtensaoMaiuscula_DeveRetornarTrue()
    {
        var content = BuildUtf8CsvContent("Nome;Descricao\r\nTeste;Desc\r\n");
        var fileMock = BuildFileMock(content, "DADOS.CSV");

        var sut = CreateSut();
        var result = sut.ValidateFile(fileMock.Object);

        Assert.True(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    #endregion
}
