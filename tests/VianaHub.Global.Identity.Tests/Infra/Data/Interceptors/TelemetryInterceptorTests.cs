using VianaHub.Global.Identity.Infra.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Interceptors;

public class TelemetryInterceptorTests
{
    private readonly Mock<ILogger<TelemetryInterceptor>> _loggerMock = new();

    private TelemetryInterceptor CreateSut() => new(_loggerMock.Object);

    #region Construtor

    [Fact(DisplayName = "TelemetryInterceptor - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "TelemetryInterceptor - Deve herdar de DbCommandInterceptor")]
    [Trait("Infra.Data", "")]
    public void TelemetryInterceptor_DeveHerdarDeDbCommandInterceptor()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<DbCommandInterceptor>(sut);
    }

    [Fact(DisplayName = "TelemetryInterceptor - Não deve lançar exceção ao receber logger nulo")]
    [Trait("Infra.Data", "")]
    public void Constructor_LoggerNulo_NaoDeveLancarExcecao()
    {
        var exception = Record.Exception(() => new TelemetryInterceptor(null!));

        Assert.Null(exception);
    }

    #endregion

    #region ReaderExecuting

    [Fact(DisplayName = "ReaderExecuting - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuting_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var result = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = sut.ReaderExecuting(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "NonQueryExecuting - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public void NonQueryExecuting_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("UPDATE Tenants SET Name='x'");
        var result = InterceptionResult<int>.SuppressWithResult(0);

        var actual = sut.NonQueryExecuting(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "ScalarExecuting - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public void ScalarExecuting_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT COUNT(1) FROM Tenants");
        var result = InterceptionResult<object>.SuppressWithResult(1);

        var actual = sut.ScalarExecuting(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "ReaderExecuting - Não deve logar quando debug não está habilitado")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuting_DebugDesabilitado_NaoDeveLogar()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");

        var actual = sut.ReaderExecuting(command, CreateCommandEventData(), default);

        _loggerMock.Verify(
            x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region CommandFailed

    [Fact(DisplayName = "CommandFailed - Deve registrar log de erro")]
    [Trait("Infra.Data", "")]
    public void CommandFailed_Sucesso_DeveLogarErro()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandErrorEventData(new InvalidOperationException("Erro de teste"));

        sut.CommandFailed(command, eventData);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "CommandFailedAsync - Deve registrar log de erro")]
    [Trait("Infra.Data", "")]
    public async Task CommandFailedAsync_Sucesso_DeveLogarErro()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandErrorEventData(new InvalidOperationException("Erro async"));

        await sut.CommandFailedAsync(command, eventData);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ReaderExecutingAsync

    [Fact(DisplayName = "ReaderExecutingAsync - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutingAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var result = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = await sut.ReaderExecutingAsync(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "NonQueryExecutingAsync - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public async Task NonQueryExecutingAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("DELETE FROM Tenants WHERE Id=1");
        var result = InterceptionResult<int>.SuppressWithResult(0);

        var actual = await sut.NonQueryExecutingAsync(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    [Fact(DisplayName = "ScalarExecutingAsync - Deve registrar log e retornar resultado")]
    [Trait("Infra.Data", "")]
    public async Task ScalarExecutingAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT MAX(Id) FROM Tenants");
        var result = InterceptionResult<object>.SuppressWithResult(42);

        var actual = await sut.ScalarExecutingAsync(command, CreateCommandEventData(), result);

        Assert.Equal(result, actual);
    }

    #endregion

    #region ReaderExecuted - Sucesso

    [Fact(DisplayName = "ReaderExecuted - Deve retornar o reader recebido")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuted_Sucesso_DeveRetornarReader()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var actual = sut.ReaderExecuted(command, eventData, null!);

        Assert.Null(actual);
    }

    [Fact(DisplayName = "ReaderExecuted - Deve logar em Debug quando duração é rápida")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuted_DuracaoRapida_DeveLogarDebug()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        sut.ReaderExecuted(command, eventData, null!);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "ReaderExecuted - Deve logar Warning quando query é lenta (> 1000ms)")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuted_QueryLenta_DeveLogarWarning()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(1500));

        sut.ReaderExecuted(command, eventData, null!);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "ReaderExecuted - Não deve logar quando nível de log não está habilitado")]
    [Trait("Infra.Data", "")]
    public void ReaderExecuted_NivelLogDesabilitado_NaoDeveLogar()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var exception = Record.Exception(() => sut.ReaderExecuted(command, eventData, null!));

        Assert.Null(exception);
        _loggerMock.Verify(
            x => x.Log(It.IsIn(LogLevel.Debug, LogLevel.Information), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region ReaderExecutedAsync - Sucesso

    [Fact(DisplayName = "ReaderExecutedAsync - Deve retornar o reader recebido")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutedAsync_Sucesso_DeveRetornarReader()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var actual = await sut.ReaderExecutedAsync(command, eventData, null!);

        Assert.Null(actual);
    }

    [Fact(DisplayName = "ReaderExecutedAsync - Deve logar em Debug quando duração é rápida")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutedAsync_DuracaoRapida_DeveLogarDebug()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(100));

        await sut.ReaderExecutedAsync(command, eventData, null!);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "ReaderExecutedAsync - Deve logar Warning quando query é lenta (> 1000ms)")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutedAsync_QueryLenta_DeveLogarWarning()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(2000));

        await sut.ReaderExecutedAsync(command, eventData, null!);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region NonQueryExecuted - Sucesso

    [Fact(DisplayName = "NonQueryExecuted - Deve retornar o resultado recebido")]
    [Trait("Infra.Data", "")]
    public void NonQueryExecuted_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("UPDATE Tenants SET Name='x'");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var actual = sut.NonQueryExecuted(command, eventData, 5);

        Assert.Equal(5, actual);
    }

    [Fact(DisplayName = "NonQueryExecuted - Deve logar em Debug quando duração é rápida")]
    [Trait("Infra.Data", "")]
    public void NonQueryExecuted_DuracaoRapida_DeveLogarDebug()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("DELETE FROM Tenants WHERE Id=1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(100));

        sut.NonQueryExecuted(command, eventData, 1);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "NonQueryExecuted - Deve logar Warning quando query é lenta (> 1000ms)")]
    [Trait("Infra.Data", "")]
    public void NonQueryExecuted_QueryLenta_DeveLogarWarning()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("DELETE FROM Tenants WHERE Id=1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(1200));

        sut.NonQueryExecuted(command, eventData, 1);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region NonQueryExecutedAsync - Sucesso

    [Fact(DisplayName = "NonQueryExecutedAsync - Deve retornar o resultado recebido")]
    [Trait("Infra.Data", "")]
    public async Task NonQueryExecutedAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("INSERT INTO Tenants VALUES('x')");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var actual = await sut.NonQueryExecutedAsync(command, eventData, 3);

        Assert.Equal(3, actual);
    }

    [Fact(DisplayName = "NonQueryExecutedAsync - Deve logar em Debug quando duração é rápida")]
    [Trait("Infra.Data", "")]
    public async Task NonQueryExecutedAsync_DuracaoRapida_DeveLogarDebug()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("INSERT INTO Tenants VALUES('x')");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(200));

        await sut.NonQueryExecutedAsync(command, eventData, 1);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "NonQueryExecutedAsync - Deve logar Warning quando query é lenta (> 1000ms)")]
    [Trait("Infra.Data", "")]
    public async Task NonQueryExecutedAsync_QueryLenta_DeveLogarWarning()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("INSERT INTO Tenants VALUES('x')");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(3000));

        await sut.NonQueryExecutedAsync(command, eventData, 1);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region ScalarExecuted - Sucesso

    [Fact(DisplayName = "ScalarExecuted - Deve retornar o resultado recebido")]
    [Trait("Infra.Data", "")]
    public void ScalarExecuted_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT COUNT(1) FROM Tenants");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var actual = sut.ScalarExecuted(command, eventData, 42);

        Assert.Equal(42, actual);
    }

    [Fact(DisplayName = "ScalarExecuted - Deve logar em Debug quando duração é rápida")]
    [Trait("Infra.Data", "")]
    public void ScalarExecuted_DuracaoRapida_DeveLogarDebug()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT COUNT(1) FROM Tenants");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(300));

        sut.ScalarExecuted(command, eventData, null);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "ScalarExecuted - Deve logar Warning quando query é lenta (> 1000ms)")]
    [Trait("Infra.Data", "")]
    public void ScalarExecuted_QueryLenta_DeveLogarWarning()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT COUNT(1) FROM Tenants");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(1800));

        sut.ScalarExecuted(command, eventData, null);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "ScalarExecuted - Deve retornar null quando resultado é null")]
    [Trait("Infra.Data", "")]
    public void ScalarExecuted_ResultadoNulo_DeveRetornarNull()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT MAX(Id) FROM Tenants");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var actual = sut.ScalarExecuted(command, eventData, null);

        Assert.Null(actual);
    }

    #endregion

    #region ScalarExecutedAsync - Sucesso

    [Fact(DisplayName = "ScalarExecutedAsync - Deve retornar o resultado recebido")]
    [Trait("Infra.Data", "")]
    public async Task ScalarExecutedAsync_Sucesso_DeveRetornarResultado()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT MAX(Id) FROM Tenants");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var actual = await sut.ScalarExecutedAsync(command, eventData, 99);

        Assert.Equal(99, actual);
    }

    [Fact(DisplayName = "ScalarExecutedAsync - Deve logar em Debug quando duração é rápida")]
    [Trait("Infra.Data", "")]
    public async Task ScalarExecutedAsync_DuracaoRapida_DeveLogarDebug()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT MAX(Id) FROM Tenants");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(400));

        await sut.ScalarExecutedAsync(command, eventData, null);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "ScalarExecutedAsync - Deve logar Warning quando query é lenta (> 1000ms)")]
    [Trait("Infra.Data", "")]
    public async Task ScalarExecutedAsync_QueryLenta_DeveLogarWarning()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT MAX(Id) FROM Tenants");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(2500));

        await sut.ScalarExecutedAsync(command, eventData, null);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region LogCommandExecution (testado via métodos *Executing)

    [Fact(DisplayName = "LogCommandExecution - Deve truncar SQL com mais de 200 caracteres")]
    [Trait("Infra.Data", "")]
    public void LogCommandExecution_SqlLongo_DeveTruncar()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var sqlLongo = new string('A', 250);
        var command = new FakeDbCommand(sqlLongo);

        var exception = Record.Exception(() => sut.ReaderExecuting(command, CreateCommandEventData(), default));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "LogCommandExecution - Deve aceitar SQL com exatamente 200 caracteres sem truncar")]
    [Trait("Infra.Data", "")]
    public void LogCommandExecution_SqlExato200_NaoDeveTruncar()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var sql200 = new string('B', 200);
        var command = new FakeDbCommand(sql200);

        var exception = Record.Exception(() => sut.NonQueryExecuting(command, CreateCommandEventData(), default));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "LogCommandExecution - Deve aceitar SQL vazio sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void LogCommandExecution_SqlVazio_NaoDeveLancarExcecao()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand(string.Empty);

        var exception = Record.Exception(() => sut.ScalarExecuting(command, CreateCommandEventData(), default));

        Assert.Null(exception);
    }

    #endregion

    #region LogCommandExecuted (testado via métodos *Executed)

    [Fact(DisplayName = "LogCommandExecuted - Deve truncar SQL longo em query lenta")]
    [Trait("Infra.Data", "")]
    public void LogCommandExecuted_SqlLongoQueryLenta_DeveTruncar()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var sqlLongo = new string('X', 250);
        var command = new FakeDbCommand(sqlLongo);
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(1500));

        var exception = Record.Exception(() => sut.ReaderExecuted(command, eventData, null!));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "LogCommandExecuted - Não deve logar quando log está desabilitado para query rápida")]
    [Trait("Infra.Data", "")]
    public void LogCommandExecuted_LogDesabilitadoQueryRapida_NaoDeveLogar()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(false);
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        sut.NonQueryExecuted(command, eventData, 0);

        _loggerMock.Verify(
            x => x.Log(It.IsIn(LogLevel.Debug, LogLevel.Information), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region LogCommandFailed (testado via CommandFailed / CommandFailedAsync)

    [Fact(DisplayName = "LogCommandFailed - Deve truncar SQL com mais de 200 caracteres")]
    [Trait("Infra.Data", "")]
    public void LogCommandFailed_SqlLongo_DeveTruncar()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        var sut = CreateSut();
        var sqlLongo = new string('Z', 300);
        var command = new FakeDbCommand(sqlLongo);
        var eventData = CreateCommandErrorEventData(new InvalidOperationException("Falha"));

        var exception = Record.Exception(() => sut.CommandFailed(command, eventData));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "LogCommandFailed - Deve incluir mensagem da exceção no log")]
    [Trait("Infra.Data", "")]
    public void LogCommandFailed_ComExcecao_DeveLogarMensagemErro()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var exception = new InvalidOperationException("Mensagem de erro crítico");
        var eventData = CreateCommandErrorEventData(exception);

        sut.CommandFailed(command, eventData);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region RecordMetrics (testado via métodos *Executed)

    [Fact(DisplayName = "RecordMetrics - Não deve lançar exceção ao registrar métricas em ReaderExecuted")]
    [Trait("Infra.Data", "")]
    public void RecordMetrics_ViaReaderExecuted_NaoDeveLancarExcecao()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var exception = Record.Exception(() => sut.ReaderExecuted(command, eventData, null!));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "RecordMetrics - Não deve lançar exceção ao registrar métricas em NonQueryExecuted")]
    [Trait("Infra.Data", "")]
    public void RecordMetrics_ViaNonQueryExecuted_NaoDeveLancarExcecao()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("UPDATE x SET y=1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var exception = Record.Exception(() => sut.NonQueryExecuted(command, eventData, 0));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "RecordMetrics - Não deve lançar exceção ao registrar métricas em ScalarExecuted")]
    [Trait("Infra.Data", "")]
    public void RecordMetrics_ViaScalarExecuted_NaoDeveLancarExcecao()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(false);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT COUNT(1) FROM x");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(50));

        var exception = Record.Exception(() => sut.ScalarExecuted(command, eventData, null));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "RecordMetrics - Deve registrar alerta de query lenta via ReaderExecuted")]
    [Trait("Infra.Data", "")]
    public void RecordMetrics_QueryLentaViaReaderExecuted_DeveLogarWarning()
    {
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandExecutedEventData(TimeSpan.FromMilliseconds(1001));

        sut.ReaderExecuted(command, eventData, null!);

        _loggerMock.Verify(
            x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region RecordErrorMetrics (testado via CommandFailed / CommandFailedAsync)

    [Fact(DisplayName = "RecordErrorMetrics - Não deve lançar exceção ao registrar métricas de erro via CommandFailed")]
    [Trait("Infra.Data", "")]
    public void RecordErrorMetrics_ViaCommandFailed_NaoDeveLancarExcecao()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandErrorEventData(new TimeoutException("Timeout"));

        var exception = Record.Exception(() => sut.CommandFailed(command, eventData));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "RecordErrorMetrics - Não deve lançar exceção ao registrar métricas de erro via CommandFailedAsync")]
    [Trait("Infra.Data", "")]
    public async Task RecordErrorMetrics_ViaCommandFailedAsync_NaoDeveLancarExcecao()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var eventData = CreateCommandErrorEventData(new TimeoutException("Timeout async"));

        var exception = await Record.ExceptionAsync(() => sut.CommandFailedAsync(command, eventData));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "RecordErrorMetrics - Deve registrar métricas para diferentes tipos de exceção")]
    [Trait("Infra.Data", "")]
    public void RecordErrorMetrics_DiferentesExcecoes_NaoDeveLancarExcecao()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");

        var exceptions = new Exception[]
        {
            new InvalidOperationException("IO"),
            new TimeoutException("Timeout"),
            new ArgumentException("Arg")
        };

        foreach (var ex in exceptions)
        {
            var eventData = CreateCommandErrorEventData(ex);
            var recordException = Record.Exception(() => sut.CommandFailed(command, eventData));
            Assert.Null(recordException);
        }
    }

    #endregion

    #region Helpers

    private static CommandEventData CreateCommandEventData()
    {
        return (CommandEventData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(CommandEventData));
    }

    private static CommandExecutedEventData CreateCommandExecutedEventData(TimeSpan duration)
    {
        var data = (CommandExecutedEventData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(CommandExecutedEventData));

        var type = typeof(CommandExecutedEventData);
        System.Reflection.FieldInfo? field = null;
        while (type != null && field is null)
        {
            field = type.GetField("<Duration>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            type = type.BaseType;
        }
        field?.SetValue(data, duration);

        return data;
    }

    private static CommandErrorEventData CreateCommandErrorEventData(Exception exception)
    {
        var data = (CommandErrorEventData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(CommandErrorEventData));

        var field = typeof(CommandErrorEventData)
            .GetField("<Exception>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(data, exception);

        return data;
    }

    #endregion
}
