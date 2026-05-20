using System.Diagnostics;
using VianaHub.Global.Identity.Infra.Data.Telemetry;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Telemetry;

public class DatabaseTelemetryTests
{
    #region ActivitySource

    [Fact(DisplayName = "ActivitySource - Deve estar inicializado com nome correto")]
    [Trait("Infra.Data", "")]
    public void ActivitySource_Inicializado_DeveConterNomeCorreto()
    {
        Assert.NotNull(DatabaseTelemetry.ActivitySource);
        Assert.Equal("VianaHub.Global.Gerit.Database", DatabaseTelemetry.ActivitySource.Name);
    }

    [Fact(DisplayName = "ActivitySource - Deve estar inicializado com versão correta")]
    [Trait("Infra.Data", "")]
    public void ActivitySource_Inicializado_DeveConterVersaoCorreta()
    {
        Assert.Equal("1.0.0", DatabaseTelemetry.ActivitySource.Version);
    }

    #endregion

    #region Counters e Histograms

    [Fact(DisplayName = "QueriesExecuted - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void QueriesExecuted_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.QueriesExecuted);
    }

    [Fact(DisplayName = "QueriesWithError - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void QueriesWithError_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.QueriesWithError);
    }

    [Fact(DisplayName = "TransactionsCommitted - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void TransactionsCommitted_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.TransactionsCommitted);
    }

    [Fact(DisplayName = "TransactionsRolledBack - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void TransactionsRolledBack_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.TransactionsRolledBack);
    }

    [Fact(DisplayName = "ConnectionsOpened - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void ConnectionsOpened_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.ConnectionsOpened);
    }

    [Fact(DisplayName = "ConnectionsClosed - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void ConnectionsClosed_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.ConnectionsClosed);
    }

    [Fact(DisplayName = "QueryDuration - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void QueryDuration_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.QueryDuration);
    }

    [Fact(DisplayName = "TransactionDuration - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void TransactionDuration_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.TransactionDuration);
    }

    [Fact(DisplayName = "ConnectionDuration - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void ConnectionDuration_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.ConnectionDuration);
    }

    [Fact(DisplayName = "ActiveConnections - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void ActiveConnections_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.ActiveConnections);
    }

    [Fact(DisplayName = "ActiveTransactions - Deve estar inicializado")]
    [Trait("Infra.Data", "")]
    public void ActiveTransactions_Inicializado_NaoDeveSerNulo()
    {
        Assert.NotNull(DatabaseTelemetry.ActiveTransactions);
    }

    #endregion

    #region IncrementActiveConnections / DecrementActiveConnections

    [Fact(DisplayName = "IncrementActiveConnections - Deve incrementar sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void IncrementActiveConnections_Sucesso_NaoDeveLancarExcecao()
    {
        var exception = Record.Exception(() => DatabaseTelemetry.IncrementActiveConnections());

        Assert.Null(exception);
    }

    [Fact(DisplayName = "DecrementActiveConnections - Deve decrementar sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void DecrementActiveConnections_Sucesso_NaoDeveLancarExcecao()
    {
        DatabaseTelemetry.IncrementActiveConnections();

        var exception = Record.Exception(() => DatabaseTelemetry.DecrementActiveConnections());

        Assert.Null(exception);
    }

    [Fact(DisplayName = "IncrementActiveConnections - Deve ser thread-safe em chamadas concorrentes")]
    [Trait("Infra.Data", "")]
    public void IncrementActiveConnections_ChamadasConcorrentes_DeveSerThreadSafe()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => DatabaseTelemetry.IncrementActiveConnections()))
            .ToArray();

        var exception = Record.Exception(() => Task.WaitAll(tasks));

        Assert.Null(exception);

        // Cleanup
        for (var i = 0; i < 10; i++)
            DatabaseTelemetry.DecrementActiveConnections();
    }

    [Fact(DisplayName = "DecrementActiveConnections - Deve ser thread-safe em chamadas concorrentes")]
    [Trait("Infra.Data", "")]
    public void DecrementActiveConnections_ChamadasConcorrentes_DeveSerThreadSafe()
    {
        for (var i = 0; i < 10; i++)
            DatabaseTelemetry.IncrementActiveConnections();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => DatabaseTelemetry.DecrementActiveConnections()))
            .ToArray();

        var exception = Record.Exception(() => Task.WaitAll(tasks));

        Assert.Null(exception);
    }

    #endregion

    #region IncrementActiveTransactions / DecrementActiveTransactions

    [Fact(DisplayName = "IncrementActiveTransactions - Deve incrementar sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void IncrementActiveTransactions_Sucesso_NaoDeveLancarExcecao()
    {
        var exception = Record.Exception(() => DatabaseTelemetry.IncrementActiveTransactions());

        Assert.Null(exception);

        // Cleanup
        DatabaseTelemetry.DecrementActiveTransactions();
    }

    [Fact(DisplayName = "DecrementActiveTransactions - Deve decrementar sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void DecrementActiveTransactions_Sucesso_NaoDeveLancarExcecao()
    {
        DatabaseTelemetry.IncrementActiveTransactions();

        var exception = Record.Exception(() => DatabaseTelemetry.DecrementActiveTransactions());

        Assert.Null(exception);
    }

    [Fact(DisplayName = "IncrementActiveTransactions - Deve ser thread-safe em chamadas concorrentes")]
    [Trait("Infra.Data", "")]
    public void IncrementActiveTransactions_ChamadasConcorrentes_DeveSerThreadSafe()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => DatabaseTelemetry.IncrementActiveTransactions()))
            .ToArray();

        var exception = Record.Exception(() => Task.WaitAll(tasks));

        Assert.Null(exception);

        // Cleanup
        for (var i = 0; i < 10; i++)
            DatabaseTelemetry.DecrementActiveTransactions();
    }

    [Fact(DisplayName = "DecrementActiveTransactions - Deve ser thread-safe em chamadas concorrentes")]
    [Trait("Infra.Data", "")]
    public void DecrementActiveTransactions_ChamadasConcorrentes_DeveSerThreadSafe()
    {
        for (var i = 0; i < 10; i++)
            DatabaseTelemetry.IncrementActiveTransactions();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => DatabaseTelemetry.DecrementActiveTransactions()))
            .ToArray();

        var exception = Record.Exception(() => Task.WaitAll(tasks));

        Assert.Null(exception);
    }

    #endregion

    #region StartDatabaseActivity - Sucesso

    [Fact(DisplayName = "StartDatabaseActivity - Deve retornar null quando não há listener registrado")]
    [Trait("Infra.Data", "")]
    public void StartDatabaseActivity_SemListener_DeveRetornarNull()
    {
        var activity = DatabaseTelemetry.StartDatabaseActivity("query");

        Assert.Null(activity);
    }

    [Fact(DisplayName = "StartDatabaseActivity - Deve retornar Activity com tags corretas quando há listener")]
    [Trait("Infra.Data", "")]
    public void StartDatabaseActivity_ComListener_DeveRetornarActivityComTags()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "VianaHub.Global.Gerit.Database",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = DatabaseTelemetry.StartDatabaseActivity("query", "users", "SELECT");

        Assert.NotNull(activity);
        Assert.Equal("db.query", activity.OperationName);
        Assert.Equal("sqlserver", activity.GetTagItem("db.system"));
        Assert.Equal("query", activity.GetTagItem("db.operation"));
        Assert.Equal("users", activity.GetTagItem("db.table"));
        Assert.Equal("SELECT", activity.GetTagItem("db.query.type"));
    }

    [Fact(DisplayName = "StartDatabaseActivity - Deve retornar Activity sem tag de tabela quando tableName é nulo")]
    [Trait("Infra.Data", "")]
    public void StartDatabaseActivity_SemTableName_NaoDeveAdicionarTagDeTabela()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "VianaHub.Global.Gerit.Database",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = DatabaseTelemetry.StartDatabaseActivity("query");

        Assert.NotNull(activity);
        Assert.Null(activity.GetTagItem("db.table"));
        Assert.Null(activity.GetTagItem("db.query.type"));
    }

    [Fact(DisplayName = "StartDatabaseActivity - Deve retornar Activity sem tag de queryType quando queryType é nulo")]
    [Trait("Infra.Data", "")]
    public void StartDatabaseActivity_SemQueryType_NaoDeveAdicionarTagDeQueryType()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "VianaHub.Global.Gerit.Database",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = DatabaseTelemetry.StartDatabaseActivity("query", "users");

        Assert.NotNull(activity);
        Assert.Equal("users", activity.GetTagItem("db.table"));
        Assert.Null(activity.GetTagItem("db.query.type"));
    }

    [Fact(DisplayName = "StartDatabaseActivity - Deve definir ActivityKind como Client")]
    [Trait("Infra.Data", "")]
    public void StartDatabaseActivity_ComListener_DeveDefinirKindComoClient()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "VianaHub.Global.Gerit.Database",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = DatabaseTelemetry.StartDatabaseActivity("insert");

        Assert.NotNull(activity);
        Assert.Equal(ActivityKind.Client, activity.Kind);
    }

    #endregion

    #region RecordError - Sucesso

    [Fact(DisplayName = "RecordError - Deve registrar erro em activity sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void RecordError_ComActivity_DeveRegistrarErroSemLancarExcecao()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "VianaHub.Global.Gerit.Database",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = DatabaseTelemetry.StartDatabaseActivity("query");
        var exception = new InvalidOperationException("Erro de teste");

        var recordException = Record.Exception(() => DatabaseTelemetry.RecordError(activity, exception));

        Assert.Null(recordException);
    }

    [Fact(DisplayName = "RecordError - Deve definir status de erro na activity")]
    [Trait("Infra.Data", "")]
    public void RecordError_ComActivity_DeveDefinirStatusDeErro()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "VianaHub.Global.Gerit.Database",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = DatabaseTelemetry.StartDatabaseActivity("query");
        var exception = new InvalidOperationException("Erro de teste");

        DatabaseTelemetry.RecordError(activity, exception);

        Assert.Equal(ActivityStatusCode.Error, activity!.Status);
        Assert.Equal("Erro de teste", activity.StatusDescription);
    }

    [Fact(DisplayName = "RecordError - Deve adicionar evento de exceção na activity")]
    [Trait("Infra.Data", "")]
    public void RecordError_ComActivity_DeveAdicionarEventoDeExcecao()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "VianaHub.Global.Gerit.Database",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = DatabaseTelemetry.StartDatabaseActivity("query");
        var exception = new InvalidOperationException("Erro de teste");

        DatabaseTelemetry.RecordError(activity, exception);

        var events = activity!.Events.ToList();
        Assert.Single(events);
        Assert.Equal("exception", events[0].Name);
    }

    #endregion

    #region RecordError - Insucesso (activity nula)

    [Fact(DisplayName = "RecordError - Deve executar sem lançar exceção quando activity é nula")]
    [Trait("Infra.Data", "")]
    public void RecordError_ActivityNula_NaoDeveLancarExcecao()
    {
        var exception = new InvalidOperationException("Erro de teste");

        var recordException = Record.Exception(() => DatabaseTelemetry.RecordError(null, exception));

        Assert.Null(recordException);
    }

    #endregion
}
