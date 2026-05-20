using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data.Common;
using System.Security.Claims;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Interceptors;

public class TenantSessionConnectionInterceptorTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<ILogger<TenantSessionConnectionInterceptor>> _loggerMock = new();
    private readonly Mock<IRequestTenantContext> _requestTenantContextMock = new();

    private TenantSessionConnectionInterceptor CreateSut() => new(
        _httpContextAccessorMock.Object,
        _loggerMock.Object,
        _requestTenantContextMock.Object);

    #region Construtor

    [Fact(DisplayName = "TenantSessionConnectionInterceptor - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "TenantSessionConnectionInterceptor - Deve herdar de DbConnectionInterceptor")]
    [Trait("Infra.Data", "")]
    public void TenantSessionConnectionInterceptor_DeveHerdarDeDbConnectionInterceptor()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<DbConnectionInterceptor>(sut);
    }

    #endregion

    #region ConnectionOpenedAsync - Conexão não SqlConnection

    [Fact(DisplayName = "ConnectionOpenedAsync - Deve retornar sem alterar SESSION_CONTEXT quando conexão não é SqlConnection")]
    [Trait("Infra.Data", "")]
    public async Task ConnectionOpenedAsync_ConexaoNaoSqlConnection_DeveRetornarSemAlterar()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(1);

        var sut = CreateSut();
        var fakeConnection = new FakeDbConnection();
        var eventData = CreateConnectionEndEventData();

        var exception = await Record.ExceptionAsync(() =>
            sut.ConnectionOpenedAsync(fakeConnection, eventData));

        Assert.Null(exception);
    }

    #endregion

    #region ConnectionOpenedAsync - Sem HttpContext com IRequestTenantContext

    [Fact(DisplayName = "ConnectionOpenedAsync - Deve retornar sem lan\u00e7ar exce\u00e7\u00e3o quando sem HttpContext e IRequestTenantContext tem TenantId")]
    [Trait("Infra.Data", "")]
    public async Task ConnectionOpenedAsync_SemHttpContextComRequestContext_DeveRetornarSemExcecao()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(7);

        var sut = CreateSut();
        var fakeConnection = new FakeDbConnection();
        var eventData = CreateConnectionEndEventData();

        var exception = await Record.ExceptionAsync(() =>
            sut.ConnectionOpenedAsync(fakeConnection, eventData));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "ConnectionOpenedAsync - Deve retornar sem lan\u00e7ar exce\u00e7\u00e3o quando sem HttpContext e sem IRequestTenantContext")]
    [Trait("Infra.Data", "")]
    public async Task ConnectionOpenedAsync_SemHttpContextSemRequestContext_DeveRetornarSemExcecao()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns((int?)null);

        var sut = CreateSut();
        var fakeConnection = new FakeDbConnection();
        var eventData = CreateConnectionEndEventData();

        var exception = await Record.ExceptionAsync(() =>
            sut.ConnectionOpenedAsync(fakeConnection, eventData));

        Assert.Null(exception);
    }

    #endregion

    #region ConnectionOpenedAsync - Usuário autenticado sem claim válida

    [Fact(DisplayName = "ConnectionOpenedAsync - Deve retornar sem lan\u00e7ar exce\u00e7\u00e3o quando usuário autenticado sem claim tenant_id")]
    [Trait("Infra.Data", "")]
    public async Task ConnectionOpenedAsync_UsuarioAutenticadoSemClaimTenant_DeveRetornarSemExcecao()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user") }, "Bearer"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = CreateSut();
        var fakeConnection = new FakeDbConnection();
        var eventData = CreateConnectionEndEventData();

        var exception = await Record.ExceptionAsync(() =>
            sut.ConnectionOpenedAsync(fakeConnection, eventData));

        Assert.Null(exception);
    }

    #endregion

    #region ConnectionOpenedAsync - Usuário não autenticado

    [Fact(DisplayName = "ConnectionOpenedAsync - Deve retornar sem lan\u00e7ar exce\u00e7\u00e3o quando request não autenticado sem IRequestTenantContext")]
    [Trait("Infra.Data", "")]
    public async Task ConnectionOpenedAsync_NaoAutenticadoSemRequestContext_DeveRetornarSemExcecao()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns((int?)null);

        var sut = CreateSut();
        var fakeConnection = new FakeDbConnection();
        var eventData = CreateConnectionEndEventData();

        var exception = await Record.ExceptionAsync(() =>
            sut.ConnectionOpenedAsync(fakeConnection, eventData));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "ConnectionOpenedAsync - Deve retornar sem lan\u00e7ar exce\u00e7\u00e3o quando request não autenticado com IRequestTenantContext definido")]
    [Trait("Infra.Data", "")]
    public async Task ConnectionOpenedAsync_NaoAutenticadoComRequestContext_DeveRetornarSemExcecao()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(3);

        var sut = CreateSut();
        var fakeConnection = new FakeDbConnection();
        var eventData = CreateConnectionEndEventData();

        var exception = await Record.ExceptionAsync(() =>
            sut.ConnectionOpenedAsync(fakeConnection, eventData));

        Assert.Null(exception);
    }

    #endregion

    #region Helpers

    private static ConnectionEndEventData CreateConnectionEndEventData()
    {
        return (ConnectionEndEventData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(ConnectionEndEventData));
    }

    #endregion
}

/// <summary>
/// Implementação fake de DbConnection para testes do TenantSessionConnectionInterceptor.
/// Não é SqlConnection, portanto os interceptors devem ignorar a interceptação.
/// </summary>
public sealed class FakeDbConnection : DbConnection
{
    public override string ConnectionString { get; set; } = string.Empty;
    public override string Database => string.Empty;
    public override string DataSource => string.Empty;
    public override string ServerVersion => string.Empty;
    public override System.Data.ConnectionState State => System.Data.ConnectionState.Open;
    public override void ChangeDatabase(string databaseName) { }
    public override void Close() { }
    public override void Open() { }
    protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) => null!;
    protected override DbCommand CreateDbCommand() => new FakeDbCommand();
}
