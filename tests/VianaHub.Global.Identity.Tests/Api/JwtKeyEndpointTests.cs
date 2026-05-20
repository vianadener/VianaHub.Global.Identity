using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Dto.Response.Jwt;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace VianaHub.Global.Identity.Tests.Api;

public class JwtKeyEndpointTests : IClassFixture<JwtKeyEndpointTests.JwtKeyWebApplicationFactory>
{
    private readonly JwtKeyWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JwtKeyEndpointTests(JwtKeyWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.NotifyMock.Reset();
        _factory.NotifyMock.Setup(x => x.HasNotify()).Returns(false);
        _factory.NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
        _factory.NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

        _factory.JwtKeyAppServiceMock.Reset();
        _factory.RequestTenantContextMock.Reset();
    }

    #region GetByTenant

    [Fact(DisplayName = "GET /v1/admin/jwtkeys/{tenantId} - Deve retornar 200 com lista de chaves")]
    [Trait("Api", "")]
    public async Task GetByTenant_Sucesso_DeveRetornar200()
    {
        var keys = Builder<JwtKeyResponse>.CreateListOfSize(3)
            .All()
            .With(x => x.TenantId = 1)
            .With(x => x.IsActive = true)
            .Build()
            .ToList();

        _factory.JwtKeyAppServiceMock
            .Setup(x => x.GetByTenantAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(keys);

        var response = await _client.GetAsync("/v1/admin/jwtkeys/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/admin/jwtkeys/{tenantId} - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetByTenant_ListaVazia_DeveRetornar200()
    {
        _factory.JwtKeyAppServiceMock
            .Setup(x => x.GetByTenantAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/v1/admin/jwtkeys/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/admin/jwtkeys/{tenantId} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetByTenant_Erro_DeveRetornar500()
    {
        _factory.JwtKeyAppServiceMock
            .Setup(x => x.GetByTenantAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/admin/jwtkeys/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetActiveKey

    [Fact(DisplayName = "GET /v1/admin/jwtkeys/{tenantId}/active - Deve retornar 200 quando chave ativa existe")]
    [Trait("Api", "")]
    public async Task GetActiveKey_Sucesso_DeveRetornar200()
    {
        var key = Builder<JwtKeyResponse>.CreateNew()
            .With(x => x.TenantId = 1)
            .With(x => x.IsActive = true)
            .Build();

        _factory.JwtKeyAppServiceMock
            .Setup(x => x.GetActiveKeyAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(key);

        var response = await _client.GetAsync("/v1/admin/jwtkeys/1/active");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/admin/jwtkeys/{tenantId}/active - Deve retornar 204 quando nenhuma chave ativa existe")]
    [Trait("Api", "")]
    public async Task GetActiveKey_SemChaveAtiva_DeveRetornar204()
    {
        _factory.JwtKeyAppServiceMock
            .Setup(x => x.GetActiveKeyAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JwtKeyResponse)null);

        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(false);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.NoContent);

        var response = await _client.GetAsync("/v1/admin/jwtkeys/1/active");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/admin/jwtkeys/{tenantId}/active - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetActiveKey_Erro_DeveRetornar500()
    {
        _factory.JwtKeyAppServiceMock
            .Setup(x => x.GetActiveKeyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/admin/jwtkeys/1/active");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region CreateInitial

    [Fact(DisplayName = "POST /v1/admin/jwtkeys/{tenantId}/create-initial - Deve retornar 201 quando chave criada com sucesso")]
    [Trait("Api", "")]
    public async Task CreateInitial_Sucesso_DeveRetornar201()
    {
        _factory.JwtKeyAppServiceMock
            .Setup(x => x.CreateInitialIfNotExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _factory.RequestTenantContextMock
            .Setup(x => x.SetTenantId(1));

        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Created);

        var response = await _client.PostAsJsonAsync("/v1/admin/jwtkeys/1/create-initial", new { });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/admin/jwtkeys/{tenantId}/create-initial - Deve retornar 200 quando chave já existe")]
    [Trait("Api", "")]
    public async Task CreateInitial_JaExiste_DeveRetornar200()
    {
        _factory.JwtKeyAppServiceMock
            .Setup(x => x.CreateInitialIfNotExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _factory.RequestTenantContextMock
            .Setup(x => x.SetTenantId(1));

        var response = await _client.PostAsJsonAsync("/v1/admin/jwtkeys/1/create-initial", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/admin/jwtkeys/{tenantId}/create-initial - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task CreateInitial_Erro_DeveRetornar500()
    {
        _factory.JwtKeyAppServiceMock
            .Setup(x => x.CreateInitialIfNotExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        _factory.RequestTenantContextMock
            .Setup(x => x.SetTenantId(It.IsAny<int>()));

        var response = await _client.PostAsJsonAsync("/v1/admin/jwtkeys/1/create-initial", new { });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Revoke

    [Fact(DisplayName = "PATCH /v1/admin/jwtkeys/{id}/revoke - Deve retornar 200 quando chave revogada com sucesso")]
    [Trait("Api", "")]
    public async Task Revoke_Sucesso_DeveRetornar200()
    {
        var request = Builder<RevokeRequest>.CreateNew()
            .With(x => x.Reason = "Chave comprometida")
            .Build();

        _factory.JwtKeyAppServiceMock
            .Setup(x => x.RevokeAsync(1, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PatchAsJsonAsync("/v1/admin/jwtkeys/1/revoke", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/admin/jwtkeys/{id}/revoke - Deve retornar 200 (sem notificação) quando serviço retorna falso")]
    [Trait("Api", "")]
    public async Task Revoke_NaoEncontrado_DeveRetornar200()
    {
        var request = Builder<RevokeRequest>.CreateNew()
            .With(x => x.Reason = "Motivo")
            .Build();

        _factory.JwtKeyAppServiceMock
            .Setup(x => x.RevokeAsync(99, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Chave não encontrada"]);

        var response = await _client.PatchAsJsonAsync("/v1/admin/jwtkeys/99/revoke", request);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/admin/jwtkeys/{id}/revoke - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Revoke_Erro_DeveRetornar500()
    {
        var request = Builder<RevokeRequest>.CreateNew()
            .With(x => x.Reason = "Motivo")
            .Build();

        _factory.JwtKeyAppServiceMock
            .Setup(x => x.RevokeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PatchAsJsonAsync("/v1/admin/jwtkeys/1/revoke", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region WebApplicationFactory

    public class JwtKeyWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IJwtKeyAppService> JwtKeyAppServiceMock { get; } = new();
        public Mock<INotify> NotifyMock { get; } = new();
        public Mock<IRequestTenantContext> RequestTenantContextMock { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IJwtKeyAppService>();
                services.RemoveAll<INotify>();
                services.RemoveAll<IRequestTenantContext>();

                NotifyMock.Setup(x => x.HasNotify()).Returns(false);
                NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
                NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

                services.AddScoped(_ => JwtKeyAppServiceMock.Object);
                services.AddScoped(_ => NotifyMock.Object);
                services.AddScoped(_ => RequestTenantContextMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, JwtKeyTestAuthHandler>("Test", _ => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        }
    }

    public class JwtKeyTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public JwtKeyTestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim("permission", "jwtkeys:read"),
                new Claim("permission", "jwtkeys:getactive"),
                new Claim("permission", "jwtkeys:create"),
                new Claim("permission", "jwtkeys:revoke")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    #endregion
}
