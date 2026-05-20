using VianaHub.Global.Identity.Api.i18n;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Net;
using System.Security.Claims;

namespace VianaHub.Global.Identity.Tests.Api;

public class CurrentUserApiServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentUserApiService _service;

    public CurrentUserApiServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _service = new CurrentUserApiService(_httpContextAccessorMock.Object);
    }

    private static DefaultHttpContext CriarHttpContextAutenticado(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }

    private static DefaultHttpContext CriarHttpContextSemAutenticacao()
    {
        return new DefaultHttpContext();
    }

    #region GetUserId

    [Fact(DisplayName = "GetUserId - Deve retornar userId a partir do claim NameIdentifier")]
    [Trait("Api", "")]
    public void GetUserId_ClaimNameIdentifier_DeveRetornarId()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim(ClaimTypes.NameIdentifier, "42")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserId();

        Assert.Equal(42, result);
    }

    [Fact(DisplayName = "GetUserId - Deve retornar userId a partir do claim sub")]
    [Trait("Api", "")]
    public void GetUserId_ClaimSub_DeveRetornarId()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim("sub", "10")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserId();

        Assert.Equal(10, result);
    }

    [Fact(DisplayName = "GetUserId - Deve retornar userId a partir do claim userId")]
    [Trait("Api", "")]
    public void GetUserId_ClaimUserId_DeveRetornarId()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim("userId", "99")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserId();

        Assert.Equal(99, result);
    }

    [Fact(DisplayName = "GetUserId - Deve retornar userId a partir do header x-user-id quando não autenticado")]
    [Trait("Api", "")]
    public void GetUserId_Header_DeveRetornarId()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Request.Headers["x-user-id"] = "55";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserId();

        Assert.Equal(55, result);
    }

    [Fact(DisplayName = "GetUserId - Deve retornar 0 quando não há claim nem header")]
    [Trait("Api", "")]
    public void GetUserId_SemClaimESemHeader_DeveRetornarZero()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserId();

        Assert.Equal(0, result);
    }

    [Fact(DisplayName = "GetUserId - Deve lançar exceção quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void GetUserId_HttpContextNulo_DeveLancarExcecao()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        Assert.Throws<Exception>(() => _service.GetUserId());
    }

    #endregion

    #region GetTenantId

    [Fact(DisplayName = "GetTenantId - Deve retornar tenantId a partir do claim tenant_id")]
    [Trait("Api", "")]
    public void GetTenantId_ClaimTenantId_DeveRetornarId()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim("tenant_id", "7")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetTenantId();

        Assert.Equal(7, result);
    }

    [Fact(DisplayName = "GetTenantId - Deve retornar tenantId a partir do claim tenantId")]
    [Trait("Api", "")]
    public void GetTenantId_ClaimTenantIdCamelCase_DeveRetornarId()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim("tenantId", "8")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetTenantId();

        Assert.Equal(8, result);
    }

    [Fact(DisplayName = "GetTenantId - Deve retornar tenantId a partir do header x-tenant-id quando não autenticado")]
    [Trait("Api", "")]
    public void GetTenantId_Header_DeveRetornarId()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Request.Headers["x-tenant-id"] = "3";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetTenantId();

        Assert.Equal(3, result);
    }

    [Fact(DisplayName = "GetTenantId - Deve retornar 1 como valor default quando não há claim nem header")]
    [Trait("Api", "")]
    public void GetTenantId_SemClaimESemHeader_DeveRetornarDefault()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetTenantId();

        Assert.Equal(1, result);
    }

    [Fact(DisplayName = "GetTenantId - Deve lançar exceção quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void GetTenantId_HttpContextNulo_DeveLancarExcecao()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        Assert.Throws<Exception>(() => _service.GetTenantId());
    }

    #endregion

    #region GetAppId

    [Fact(DisplayName = "GetAppId - Deve retornar appId a partir do claim app_id")]
    [Trait("Api", "")]
    public void GetAppId_ClaimAppId_DeveRetornarId()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim("app_id", "5")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetAppId();

        Assert.Equal(5, result);
    }

    [Fact(DisplayName = "GetAppId - Deve retornar appId a partir do header x-app-id quando não autenticado")]
    [Trait("Api", "")]
    public void GetAppId_Header_DeveRetornarId()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Request.Headers["x-app-id"] = "2";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetAppId();

        Assert.Equal(2, result);
    }

    [Fact(DisplayName = "GetAppId - Deve retornar 1 como valor default quando não há claim nem header")]
    [Trait("Api", "")]
    public void GetAppId_SemClaimESemHeader_DeveRetornarDefault()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetAppId();

        Assert.Equal(1, result);
    }

    [Fact(DisplayName = "GetAppId - Deve lançar exceção quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void GetAppId_HttpContextNulo_DeveLancarExcecao()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        Assert.Throws<Exception>(() => _service.GetAppId());
    }

    #endregion

    #region GetUserName

    [Fact(DisplayName = "GetUserName - Deve retornar nome a partir do claim Name")]
    [Trait("Api", "")]
    public void GetUserName_ClaimName_DeveRetornarNome()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim(ClaimTypes.Name, "João Silva")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserName();

        Assert.Equal("João Silva", result);
    }

    [Fact(DisplayName = "GetUserName - Deve retornar nome a partir do claim preferred_username")]
    [Trait("Api", "")]
    public void GetUserName_ClaimPreferredUsername_DeveRetornarNome()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim("preferred_username", "joao.silva")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserName();

        Assert.Equal("joao.silva", result);
    }

    [Fact(DisplayName = "GetUserName - Deve retornar nome a partir do header x-user-name quando não autenticado")]
    [Trait("Api", "")]
    public void GetUserName_Header_DeveRetornarNome()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Request.Headers["x-user-name"] = "maria.souza";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserName();

        Assert.Equal("maria.souza", result);
    }

    [Fact(DisplayName = "GetUserName - Deve retornar string vazia quando não há claim nem header")]
    [Trait("Api", "")]
    public void GetUserName_SemClaimESemHeader_DeveRetornarVazio()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserName();

        Assert.Equal(string.Empty, result);
    }

    [Fact(DisplayName = "GetUserName - Deve retornar string vazia quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void GetUserName_HttpContextNulo_DeveRetornarVazio()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        var result = _service.GetUserName();

        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region GetUserEmail

    [Fact(DisplayName = "GetUserEmail - Deve retornar email a partir do claim Email")]
    [Trait("Api", "")]
    public void GetUserEmail_ClaimEmail_DeveRetornarEmail()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim(ClaimTypes.Email, "joao@email.com")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserEmail();

        Assert.Equal("joao@email.com", result);
    }

    [Fact(DisplayName = "GetUserEmail - Deve retornar email a partir do claim email (minúsculo)")]
    [Trait("Api", "")]
    public void GetUserEmail_ClaimEmailMinusculo_DeveRetornarEmail()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim("email", "maria@email.com")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserEmail();

        Assert.Equal("maria@email.com", result);
    }

    [Fact(DisplayName = "GetUserEmail - Deve retornar email a partir do header x-user-email quando não autenticado")]
    [Trait("Api", "")]
    public void GetUserEmail_Header_DeveRetornarEmail()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Request.Headers["x-user-email"] = "carlos@email.com";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserEmail();

        Assert.Equal("carlos@email.com", result);
    }

    [Fact(DisplayName = "GetUserEmail - Deve retornar string vazia quando não há claim nem header")]
    [Trait("Api", "")]
    public void GetUserEmail_SemClaimESemHeader_DeveRetornarVazio()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserEmail();

        Assert.Equal(string.Empty, result);
    }

    [Fact(DisplayName = "GetUserEmail - Deve retornar string vazia quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void GetUserEmail_HttpContextNulo_DeveRetornarVazio()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        var result = _service.GetUserEmail();

        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region IsAuthenticated

    [Fact(DisplayName = "IsAuthenticated - Deve retornar true quando usuário está autenticado")]
    [Trait("Api", "")]
    public void IsAuthenticated_UsuarioAutenticado_DeveRetornarTrue()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim(ClaimTypes.Name, "test")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.IsAuthenticated();

        Assert.True(result);
    }

    [Fact(DisplayName = "IsAuthenticated - Deve retornar false quando usuário não está autenticado")]
    [Trait("Api", "")]
    public void IsAuthenticated_UsuarioNaoAutenticado_DeveRetornarFalse()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.IsAuthenticated();

        Assert.False(result);
    }

    [Fact(DisplayName = "IsAuthenticated - Deve retornar false quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void IsAuthenticated_HttpContextNulo_DeveRetornarFalse()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        var result = _service.IsAuthenticated();

        Assert.False(result);
    }

    #endregion

    #region GetUserIdentifier

    [Fact(DisplayName = "GetUserIdentifier - Deve retornar email quando disponível")]
    [Trait("Api", "")]
    public void GetUserIdentifier_ComEmail_DeveRetornarEmail()
    {
        var httpContext = CriarHttpContextAutenticado([
            new Claim(ClaimTypes.Email, "joao@email.com"),
            new Claim(ClaimTypes.Name, "joao"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        ]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserIdentifier();

        Assert.Equal("joao@email.com", result);
    }

    [Fact(DisplayName = "GetUserIdentifier - Deve retornar nome quando email não disponível")]
    [Trait("Api", "")]
    public void GetUserIdentifier_SemEmailComNome_DeveRetornarNome()
    {
        var httpContext = CriarHttpContextAutenticado([
            new Claim(ClaimTypes.Name, "joao.silva"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        ]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserIdentifier();

        Assert.Equal("joao.silva", result);
    }

    [Fact(DisplayName = "GetUserIdentifier - Deve retornar userId quando não há email nem nome")]
    [Trait("Api", "")]
    public void GetUserIdentifier_SemEmailSemNome_DeveRetornarUserId()
    {
        var httpContext = CriarHttpContextAutenticado([new Claim(ClaimTypes.NameIdentifier, "42")]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserIdentifier();

        Assert.Equal("42", result);
    }

    [Fact(DisplayName = "GetUserIdentifier - Deve retornar 'system' quando não há nenhuma informação")]
    [Trait("Api", "")]
    public void GetUserIdentifier_SemNenhumaInformacao_DeveRetornarSystem()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserIdentifier();

        Assert.Equal("system", result);
    }

    #endregion

    #region GetUser

    [Fact(DisplayName = "GetUser - Deve retornar CurrentUserContext preenchido")]
    [Trait("Api", "")]
    public void GetUser_UsuarioAutenticado_DeveRetornarContextoPreenchido()
    {
        var httpContext = CriarHttpContextAutenticado([
            new Claim(ClaimTypes.NameIdentifier, "7"),
            new Claim(ClaimTypes.Name, "Ana"),
            new Claim(ClaimTypes.Email, "ana@email.com")
        ]);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUser();

        Assert.Equal(7, result.UserId);
        Assert.Equal("Ana", result.UserName);
        Assert.Equal("ana@email.com", result.Email);
    }

    [Fact(DisplayName = "GetUser - Deve retornar CurrentUserContext com valores default quando sem autenticação")]
    [Trait("Api", "")]
    public void GetUser_SemAutenticacao_DeveRetornarContextoDefault()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUser();

        Assert.Equal(0, result.UserId);
        Assert.Equal(string.Empty, result.UserName);
        Assert.Equal(string.Empty, result.Email);
    }

    #endregion

    #region GetUserIpAddress

    [Fact(DisplayName = "GetUserIpAddress - Deve retornar IP do header X-Forwarded-For")]
    [Trait("Api", "")]
    public void GetUserIpAddress_HeaderForwardedFor_DeveRetornarIp()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Request.Headers["X-Forwarded-For"] = "192.168.1.1, 10.0.0.1";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserIpAddress();

        Assert.Equal("192.168.1.1", result);
    }

    [Fact(DisplayName = "GetUserIpAddress - Deve retornar IP da conexão quando sem header proxy")]
    [Trait("Api", "")]
    public void GetUserIpAddress_SemProxy_DeveRetornarIpConexao()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("172.16.0.5");
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserIpAddress();

        Assert.Equal("172.16.0.5", result);
    }

    [Fact(DisplayName = "GetUserIpAddress - Deve retornar 'Unknown' quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void GetUserIpAddress_HttpContextNulo_DeveRetornarUnknown()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        var result = _service.GetUserIpAddress();

        Assert.Equal("Unknown", result);
    }

    [Fact(DisplayName = "GetUserIpAddress - Deve retornar 'Unknown' quando RemoteIpAddress é nulo e sem header proxy")]
    [Trait("Api", "")]
    public void GetUserIpAddress_SemIpConexaoESemProxy_DeveRetornarUnknown()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserIpAddress();

        Assert.Equal("Unknown", result);
    }

    #endregion

    #region GetUserAgent

    [Fact(DisplayName = "GetUserAgent - Deve retornar User-Agent do header")]
    [Trait("Api", "")]
    public void GetUserAgent_ComHeader_DeveRetornarUserAgent()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        httpContext.Request.Headers["User-Agent"] = "Mozilla/5.0";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserAgent();

        Assert.Equal("Mozilla/5.0", result);
    }

    [Fact(DisplayName = "GetUserAgent - Deve retornar 'Unknown' quando header não presente")]
    [Trait("Api", "")]
    public void GetUserAgent_SemHeader_DeveRetornarUnknown()
    {
        var httpContext = CriarHttpContextSemAutenticacao();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var result = _service.GetUserAgent();

        Assert.Equal("Unknown", result);
    }

    [Fact(DisplayName = "GetUserAgent - Deve retornar 'Unknown' quando HttpContext é nulo")]
    [Trait("Api", "")]
    public void GetUserAgent_HttpContextNulo_DeveRetornarUnknown()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);

        var result = _service.GetUserAgent();

        Assert.Equal("Unknown", result);
    }

    #endregion
}
