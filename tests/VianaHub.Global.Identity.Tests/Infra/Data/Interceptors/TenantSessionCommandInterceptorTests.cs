using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Infra.Data.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Interceptors;

public class TenantSessionCommandInterceptorTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IRequestTenantContext> _requestTenantContextMock = new();
    private readonly Mock<ILogger<TenantSessionCommandInterceptor>> _loggerMock = new();

    private TenantSessionCommandInterceptor CreateSut() => new(
        _httpContextAccessorMock.Object,
        _requestTenantContextMock.Object,
        _loggerMock.Object);

    #region Construtor

    [Fact(DisplayName = "TenantSessionCommandInterceptor - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "TenantSessionCommandInterceptor - Deve herdar de DbCommandInterceptor")]
    [Trait("Infra.Data", "")]
    public void TenantSessionCommandInterceptor_DeveHerdarDeDbCommandInterceptor()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<DbCommandInterceptor>(sut);
    }

    #endregion

    #region ReaderExecutingAsync - Sem SqlConnection (ignorar interceptação)

    [Fact(DisplayName = "ReaderExecutingAsync - Deve retornar resultado sem alterar quando conexão não é SqlConnection")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutingAsync_ConexaoNaoSqlConnection_DeveRetornarResultadoSemInterferir()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(5);

        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var expected = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = await sut.ReaderExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "ReaderExecutingAsync - Deve ignorar quando comando já é sp_set_session_context")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutingAsync_ComandoSpSetSessionContext_DeveIgnorar()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(5);

        var sut = CreateSut();
        var command = new FakeDbCommand("EXEC sp_set_session_context @key=N'TenantId', @value=@tenantId;");
        var expected = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = await sut.ReaderExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region ScalarExecutingAsync - Sem SqlConnection

    [Fact(DisplayName = "ScalarExecutingAsync - Deve retornar resultado sem alterar quando conexão não é SqlConnection")]
    [Trait("Infra.Data", "")]
    public async Task ScalarExecutingAsync_ConexaoNaoSqlConnection_DeveRetornarResultadoSemInterferir()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(1);

        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT COUNT(1)");
        var expected = InterceptionResult<object>.SuppressWithResult(42);

        var actual = await sut.ScalarExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "ScalarExecutingAsync - Deve ignorar quando comando já é sp_set_session_context")]
    [Trait("Infra.Data", "")]
    public async Task ScalarExecutingAsync_ComandoSpSetSessionContext_DeveIgnorar()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(1);

        var sut = CreateSut();
        var command = new FakeDbCommand("EXEC sp_set_session_context @key=N'TenantId', @value=@tenantId;");
        var expected = InterceptionResult<object>.SuppressWithResult(null!);

        var actual = await sut.ScalarExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region NonQueryExecutingAsync - Sem SqlConnection

    [Fact(DisplayName = "NonQueryExecutingAsync - Deve retornar resultado sem alterar quando conexão não é SqlConnection")]
    [Trait("Infra.Data", "")]
    public async Task NonQueryExecutingAsync_ConexaoNaoSqlConnection_DeveRetornarResultadoSemInterferir()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(1);

        var sut = CreateSut();
        var command = new FakeDbCommand("DELETE FROM Tenants WHERE Id=1");
        var expected = InterceptionResult<int>.SuppressWithResult(1);

        var actual = await sut.NonQueryExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "NonQueryExecutingAsync - Deve ignorar quando comando já é sp_set_session_context")]
    [Trait("Infra.Data", "")]
    public async Task NonQueryExecutingAsync_ComandoSpSetSessionContext_DeveIgnorar()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns(1);

        var sut = CreateSut();
        var command = new FakeDbCommand("EXEC sp_set_session_context @key=N'TenantId', @value=@tenantId;");
        var expected = InterceptionResult<int>.SuppressWithResult(0);

        var actual = await sut.NonQueryExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region ResolveTenantId - Sem HttpContext

    [Fact(DisplayName = "ReaderExecutingAsync - Deve retornar resultado quando sem HttpContext e sem IRequestTenantContext")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutingAsync_SemHttpContextESemRequestContext_DeveRetornarResultado()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null!);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns((int?)null);

        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var expected = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = await sut.ReaderExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region ResolveTenantId - Usuário autenticado sem claim válida

    [Fact(DisplayName = "ReaderExecutingAsync - Deve retornar resultado quando usuário autenticado sem claim tenant_id")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutingAsync_UsuarioAutenticadoSemClaimTenant_DeveRetornarResultado()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user") }, "Bearer"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var expected = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = await sut.ReaderExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region ResolveTenantId - Usuário não autenticado sem IRequestTenantContext

    [Fact(DisplayName = "ReaderExecutingAsync - Deve retornar resultado quando request não autenticado sem IRequestTenantContext")]
    [Trait("Infra.Data", "")]
    public async Task ReaderExecutingAsync_NaoAutenticadoSemRequestContext_DeveRetornarResultado()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _requestTenantContextMock.Setup(x => x.TenantId).Returns((int?)null);

        var sut = CreateSut();
        var command = new FakeDbCommand("SELECT 1");
        var expected = InterceptionResult<System.Data.Common.DbDataReader>.SuppressWithResult(null!);

        var actual = await sut.ReaderExecutingAsync(command, CreateCommandEventData(), expected);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region Helpers

    private static CommandEventData CreateCommandEventData()
    {
        return (CommandEventData)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(CommandEventData));
    }

    #endregion
}
