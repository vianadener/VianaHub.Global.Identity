using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.App;
using VianaHub.Global.Identity.Application.Dto.Response.App;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
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

public class AppEndpointTests : IClassFixture<AppEndpointTests.AppWebApplicationFactory>
{
    private readonly AppWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AppEndpointTests(AppWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.NotifyMock.Reset();
        _factory.NotifyMock.Setup(x => x.HasNotify()).Returns(false);
        _factory.NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
        _factory.NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

        _factory.AppAppServiceMock.Reset();
    }

    #region GetAll

    [Fact(DisplayName = "GET /v1/apps - Deve retornar 200 com lista de apps")]
    [Trait("Api", "")]
    public async Task GetAll_Sucesso_DeveRetornar200()
    {
        var apps = Builder<AppResponse>.CreateListOfSize(3)
            .All()
            .With(x => x.IsActive = true)
            .Build()
            .ToList();

        _factory.AppAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(apps);

        var response = await _client.GetAsync("/v1/apps/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/apps - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetAll_ListaVazia_DeveRetornar200()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/v1/apps/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/apps - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetAll_Erro_DeveRetornar500()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/apps/");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetById

    [Fact(DisplayName = "GET /v1/apps/{id} - Deve retornar 200 quando app existe")]
    [Trait("Api", "")]
    public async Task GetById_Sucesso_DeveRetornar200()
    {
        var app = Builder<AppResponse>.CreateNew()
            .With(x => x.Id = 1)
            .With(x => x.IsActive = true)
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(app);

        var response = await _client.GetAsync("/v1/apps/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/apps/{id} - Deve retornar 410 quando app não existe")]
    [Trait("Api", "")]
    public async Task GetById_NaoEncontrado_DeveRetornar410()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["App não encontrada"]);

        var response = await _client.GetAsync("/v1/apps/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/apps/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetById_Erro_DeveRetornar500()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/apps/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetPaged

    [Fact(DisplayName = "GET /v1/apps/paged - Deve retornar 200 com lista paginada")]
    [Trait("Api", "")]
    public async Task GetPaged_Sucesso_DeveRetornar200()
    {
        var items = Builder<AppResponse>.CreateListOfSize(2)
            .All()
            .With(x => x.IsActive = true)
            .Build()
            .ToList();

        var paged = new ListPageResponse<AppResponse>(items, 1, 10, 2, 1);

        _factory.AppAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PagedFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var response = await _client.GetAsync("/v1/apps/paged?PageNumber=1&PageSize=10&Search=&SortBy=Name&SortDirection=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/apps/paged - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetPaged_ListaVazia_DeveRetornar200()
    {
        var paged = new ListPageResponse<AppResponse>([], 1, 10, 0, 0);

        _factory.AppAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PagedFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var response = await _client.GetAsync("/v1/apps/paged?PageNumber=1&PageSize=10&Search=&SortBy=Name&SortDirection=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/apps/paged - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetPaged_Erro_DeveRetornar500()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PagedFilterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/apps/paged?PageNumber=1&PageSize=10&Search=&SortBy=Name&SortDirection=asc");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact(DisplayName = "POST /v1/apps - Deve retornar 201 quando app criada com sucesso")]
    [Trait("Api", "")]
    public async Task Create_Sucesso_DeveRetornar201()
    {
        var request = Builder<CreateAppRequest>.CreateNew()
            .With(x => x.Name = "App Test")
            .With(x => x.Description = "Descrição")
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateAppRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PostAsJsonAsync("/v1/apps/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/apps - Deve retornar 200 quando serviço retorna falso com notificação")]
    [Trait("Api", "")]
    public async Task Create_DadosInvalidos_DeveRetornar200()
    {
        var request = Builder<CreateAppRequest>.CreateNew()
            .With(x => x.Name = string.Empty)
            .With(x => x.Description = string.Empty)
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateAppRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Nome é obrigatório"]);

        var response = await _client.PostAsJsonAsync("/v1/apps/", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/apps - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Create_Erro_DeveRetornar500()
    {
        var request = Builder<CreateAppRequest>.CreateNew()
            .With(x => x.Name = "App Test")
            .With(x => x.Description = "Descrição")
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateAppRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PostAsJsonAsync("/v1/apps/", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Update

    [Fact(DisplayName = "PUT /v1/apps/{id} - Deve retornar 200 quando atualizado com sucesso")]
    [Trait("Api", "")]
    public async Task Update_Sucesso_DeveRetornar200()
    {
        var request = Builder<UpdateAppRequest>.CreateNew()
            .With(x => x.Name = "App Atualizada")
            .With(x => x.Description = "Descrição atualizada")
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateAppRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PutAsJsonAsync("/v1/apps/1", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/apps/{id} - Deve retornar 410 quando app não existe")]
    [Trait("Api", "")]
    public async Task Update_NaoEncontrado_DeveRetornar410()
    {
        var request = Builder<UpdateAppRequest>.CreateNew()
            .With(x => x.Name = "App")
            .With(x => x.Description = "Desc")
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.UpdateAsync(99, It.IsAny<UpdateAppRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["App não encontrada"]);

        var response = await _client.PutAsJsonAsync("/v1/apps/99", request);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/apps/{id} - Deve retornar 409 quando nome já está em uso")]
    [Trait("Api", "")]
    public async Task Update_Duplicado_DeveRetornar409()
    {
        var request = Builder<UpdateAppRequest>.CreateNew()
            .With(x => x.Name = "App Duplicada")
            .With(x => x.Description = "Desc")
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateAppRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Conflict);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Nome de app já está em uso"]);

        var response = await _client.PutAsJsonAsync("/v1/apps/1", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/apps/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Update_Erro_DeveRetornar500()
    {
        var request = Builder<UpdateAppRequest>.CreateNew()
            .With(x => x.Name = "App")
            .With(x => x.Description = "Desc")
            .Build();

        _factory.AppAppServiceMock
            .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateAppRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PutAsJsonAsync("/v1/apps/1", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Activate

    [Fact(DisplayName = "PATCH /v1/apps/{id}/activate - Deve retornar 200 quando ativado com sucesso")]
    [Trait("Api", "")]
    public async Task Activate_Sucesso_DeveRetornar200()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.ActivateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PatchAsync("/v1/apps/1/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/apps/{id}/activate - Deve retornar 410 quando app não existe")]
    [Trait("Api", "")]
    public async Task Activate_NaoEncontrado_DeveRetornar410()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.ActivateAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["App não encontrada"]);

        var response = await _client.PatchAsync("/v1/apps/99/activate", null);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/apps/{id}/activate - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Activate_Erro_DeveRetornar500()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.ActivateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PatchAsync("/v1/apps/1/activate", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Deactivate

    [Fact(DisplayName = "PATCH /v1/apps/{id}/deactivate - Deve retornar 200 quando desativado com sucesso")]
    [Trait("Api", "")]
    public async Task Deactivate_Sucesso_DeveRetornar200()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.DeactivateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PatchAsync("/v1/apps/1/deactivate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/apps/{id}/deactivate - Deve retornar 410 quando app não existe")]
    [Trait("Api", "")]
    public async Task Deactivate_NaoEncontrado_DeveRetornar410()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.DeactivateAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["App não encontrada"]);

        var response = await _client.PatchAsync("/v1/apps/99/deactivate", null);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/apps/{id}/deactivate - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Deactivate_Erro_DeveRetornar500()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.DeactivateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PatchAsync("/v1/apps/1/deactivate", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Delete

    [Fact(DisplayName = "DELETE /v1/apps/{id} - Deve retornar 200 quando excluído com sucesso")]
    [Trait("Api", "")]
    public async Task Delete_Sucesso_DeveRetornar200()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.DeleteAsync("/v1/apps/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/apps/{id} - Deve retornar 410 quando app não existe")]
    [Trait("Api", "")]
    public async Task Delete_NaoEncontrado_DeveRetornar410()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["App não encontrada"]);

        var response = await _client.DeleteAsync("/v1/apps/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/apps/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Delete_Erro_DeveRetornar500()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.DeleteAsync("/v1/apps/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region BulkUpload

    [Fact(DisplayName = "POST /v1/apps/bulk-upload - Deve retornar 200 quando upload realizado com sucesso")]
    [Trait("Api", "")]
    public async Task BulkUpload_Sucesso_DeveRetornar200()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.BulkUploadAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("id,name\n1,App1"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "apps.csv");

        var response = await _client.PostAsync("/v1/apps/bulk-upload", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/apps/bulk-upload - Deve retornar 400 quando nenhum arquivo enviado")]
    [Trait("Api", "")]
    public async Task BulkUpload_SemArquivo_DeveRetornar400()
    {
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Api.Upload.NoFileProvided"]);

        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/v1/apps/bulk-upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/apps/bulk-upload - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task BulkUpload_Erro_DeveRetornar500()
    {
        _factory.AppAppServiceMock
            .Setup(x => x.BulkUploadAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("id,name\n1,App1"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "apps.csv");

        var response = await _client.PostAsync("/v1/apps/bulk-upload", content);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region WebApplicationFactory

    public class AppWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IAppAppService> AppAppServiceMock { get; } = new();
        public Mock<INotify> NotifyMock { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAppAppService>();
                services.RemoveAll<INotify>();

                NotifyMock.Setup(x => x.HasNotify()).Returns(false);
                NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
                NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

                services.AddScoped(_ => AppAppServiceMock.Object);
                services.AddScoped(_ => NotifyMock.Object);

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });
            });
        }
    }

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim("permission", "apps:read"),
                new Claim("permission", "apps:create"),
                new Claim("permission", "apps:update"),
                new Claim("permission", "apps:activate"),
                new Claim("permission", "apps:deactivate"),
                new Claim("permission", "apps:delete"),
                new Claim("permission", "apps:bulkupload")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    #endregion
}
