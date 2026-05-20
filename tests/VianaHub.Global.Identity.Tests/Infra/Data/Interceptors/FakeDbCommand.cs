using System.Data;
using System.Data.Common;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Interceptors;

/// <summary>
/// Implementação fake de DbCommand para uso exclusivo nos testes dos interceptors.
/// </summary>
public sealed class FakeDbCommand : DbCommand
{
    public FakeDbCommand(string commandText = "")
    {
        CommandText = commandText;
    }

    public override string CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();
    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel() { }
    public override int ExecuteNonQuery() => 0;
    public override object? ExecuteScalar() => null;
    public override void Prepare() { }
    protected override DbParameter CreateDbParameter() => new FakeDbParameter();
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => null!;
}

public sealed class FakeDbParameter : DbParameter
{
    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; }
    public override bool IsNullable { get; set; }
    public override string ParameterName { get; set; } = string.Empty;
    public override int Size { get; set; }
    public override string SourceColumn { get; set; } = string.Empty;
    public override bool SourceColumnNullMapping { get; set; }
    public override object? Value { get; set; }
    public override void ResetDbType() { }
}

public sealed class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _params = [];

    public override int Count => _params.Count;
    public override object SyncRoot => _params;
    public override int Add(object value) { _params.Add((DbParameter)value); return _params.Count - 1; }
    public override void AddRange(Array values) { foreach (DbParameter p in values) _params.Add(p); }
    public override void Clear() => _params.Clear();
    public override bool Contains(object value) => _params.Contains((DbParameter)value);
    public override bool Contains(string value) => _params.Any(p => p.ParameterName == value);
    public override void CopyTo(Array array, int index) => ((System.Collections.ICollection)_params).CopyTo(array, index);
    public override System.Collections.IEnumerator GetEnumerator() => _params.GetEnumerator();
    public override int IndexOf(object value) => _params.IndexOf((DbParameter)value);
    public override int IndexOf(string parameterName) => _params.FindIndex(p => p.ParameterName == parameterName);
    public override void Insert(int index, object value) => _params.Insert(index, (DbParameter)value);
    public override void Remove(object value) => _params.Remove((DbParameter)value);
    public override void RemoveAt(int index) => _params.RemoveAt(index);
    public override void RemoveAt(string parameterName) => _params.RemoveAll(p => p.ParameterName == parameterName);
    protected override DbParameter GetParameter(int index) => _params[index];
    protected override DbParameter GetParameter(string parameterName) => _params.First(p => p.ParameterName == parameterName);
    protected override void SetParameter(int index, DbParameter value) => _params[index] = value;
    protected override void SetParameter(string parameterName, DbParameter value) { var i = IndexOf(parameterName); _params[i] = value; }
}
