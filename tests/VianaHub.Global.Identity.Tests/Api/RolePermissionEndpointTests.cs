using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Request.RolePermission;
using VianaHub.Global.Identity.Application.Dto.Response.RolePermission;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Application.Interfaces;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
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

public class RolePermissionEndpointTests : IClassFixture<RolePermissionEndpointTests.RolePermissionWebApplicationFactory>
{
    private readonly RolePermissionWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RolePermissionEndpointTests(RolePermissionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.NotifyMock.Reset();
        _factory.NotifyMock.Setup(x => x.HasNotify()).Returns(false);
        _factory.NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
        _factory.NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

        _factory.RolePermissionAppServiceMock.Reset();
    }

    #region GetAll

    [Fact(DisplayName = "GET /v1/role-permissions - Deve retornar 200 com lista de role permissions")]
    [Trait("Api", "")]
    public async Task GetAll_Sucesso_DeveRetornar200()
    {
        var permissions = Builder<RolePermissionResponse>.CreateListOfSize(3)
            .All()
            .With(x => x.Role = "Admin")
            .With(x => x.Resource = "Users")
            .With(x => x.Action = "Read")
            .Build()
            .ToList();

        _factory.RolePermissionAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var response = await _client.GetAsync("/v1/role-permissions/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/role-permissions - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetAll_ListaVazia_DeveRetornar200()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/v1/role-permissions/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/role-permissions - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetAll_Erro_DeveRetornar500()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/role-permissions/");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetById

    [Fact(DisplayName = "GET /v1/role-permissions/{id} - Deve retornar 200 quando role permission existe")]
    [Trait("Api", "")]
    public async Task GetById_Sucesso_DeveRetornar200()
    {
        var permission = Builder<RolePermissionDetailResponse>.CreateNew()
            .With(x => x.Id = 1)
            .With(x => x.Role = "Admin")
            .With(x => x.Resource = "Users")
            .With(x => x.Action = "Read")
            .Build();

        _factory.RolePermissionAppServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permission);

        var response = await _client.GetAsync("/v1/role-permissions/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/role-permissions/{id} - Deve retornar 410 quando role permission não existe")]
    [Trait("Api", "")]
    public async Task GetById_NaoEncontrado_DeveRetornar410()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RolePermissionDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["RolePermission não encontrada"]);

        var response = await _client.GetAsync("/v1/role-permissions/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/role-permissions/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetById_Erro_DeveRetornar500()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/role-permissions/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact(DisplayName = "POST /v1/role-permissions - Deve retornar 201 quando role permission criada com sucesso")]
    [Trait("Api", "")]
    public async Task Create_Sucesso_DeveRetornar201()
    {
        var request = Builder<CreateRolePermissionRequest>.CreateNew()
            .With(x => x.RoleId = 1)
            .With(x => x.ResourceId = 1)
            .With(x => x.ActionId = 1)
            .Build();

        var created = Builder<RolePermissionResponse>.CreateNew()
            .With(x => x.Id = 1)
            .Build();

        _factory.RolePermissionAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateRolePermissionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var response = await _client.PostAsJsonAsync("/v1/role-permissions/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/role-permissions - Deve retornar 400 quando serviço retorna falso com notificação")]
    [Trait("Api", "")]
    public async Task Create_DadosInvalidos_DeveRetornar400()
    {
        var request = Builder<CreateRolePermissionRequest>.CreateNew()
            .With(x => x.RoleId = 0)
            .With(x => x.ResourceId = 0)
            .With(x => x.ActionId = 0)
            .Build();

        _factory.RolePermissionAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateRolePermissionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RolePermissionResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["RoleId, ResourceId e ActionId são obrigatórios"]);

        var response = await _client.PostAsJsonAsync("/v1/role-permissions/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/role-permissions - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Create_Erro_DeveRetornar500()
    {
        var request = Builder<CreateRolePermissionRequest>.CreateNew()
            .With(x => x.RoleId = 1)
            .With(x => x.ResourceId = 1)
            .With(x => x.ActionId = 1)
            .Build();

        _factory.RolePermissionAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateRolePermissionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PostAsJsonAsync("/v1/role-permissions/", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Delete

    [Fact(DisplayName = "DELETE /v1/role-permissions/{id} - Deve retornar 200 quando excluído com sucesso")]
    [Trait("Api", "")]
    public async Task Delete_Sucesso_DeveRetornar200()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()));

        var response = await _client.DeleteAsync("/v1/role-permissions/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/role-permissions/{id} - Deve retornar 410 quando role permission não existe")]
    [Trait("Api", "")]
    public async Task Delete_NaoEncontrado_DeveRetornar410()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.DeleteAsync(99, It.IsAny<CancellationToken>()));
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["RolePermission não encontrada"]);

        var response = await _client.DeleteAsync("/v1/role-permissions/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/role-permissions/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Delete_Erro_DeveRetornar500()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.DeleteAsync("/v1/role-permissions/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region BulkUpload

    [Fact(DisplayName = "POST /v1/role-permissions/bulk-upload - Deve retornar 200 quando upload realizado com sucesso")]
    [Trait("Api", "")]
    public async Task BulkUpload_Sucesso_DeveRetornar200()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.BulkUploadAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("roleId,resourceId,actionId\n1,1,1"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "role-permissions.csv");

        var response = await _client.PostAsync("/v1/role-permissions/bulk-upload", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/role-permissions/bulk-upload - Deve retornar 400 quando nenhum arquivo enviado")]
    [Trait("Api", "")]
    public async Task BulkUpload_SemArquivo_DeveRetornar400()
    {
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Nenhum arquivo foi enviado"]);

        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/v1/role-permissions/bulk-upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/role-permissions/bulk-upload - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task BulkUpload_Erro_DeveRetornar500()
    {
        _factory.RolePermissionAppServiceMock
            .Setup(x => x.BulkUploadAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("roleId,resourceId,actionId\n1,1,1"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "role-permissions.csv");

        var response = await _client.PostAsync("/v1/role-permissions/bulk-upload", content);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region WebApplicationFactory

    public class RolePermissionWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IRolePermissionAppService> RolePermissionAppServiceMock { get; } = new();
        public Mock<INotify> NotifyMock { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRolePermissionAppService>();
                services.RemoveAll<INotify>();

                NotifyMock.Setup(x => x.HasNotify()).Returns(false);
                NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
                NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

                services.AddScoped(_ => RolePermissionAppServiceMock.Object);
                services.AddScoped(_ => NotifyMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, RolePermissionTestAuthHandler>("Test", _ => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        }
    }

    public class RolePermissionTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public RolePermissionTestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("permission", "rolepermissions:read"),
                new Claim("permission", "rolepermissions:create"),
                new Claim("permission", "rolepermissions:delete"),
                new Claim("permission", "rolepermissions:bulkupload")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    #endregion
}
