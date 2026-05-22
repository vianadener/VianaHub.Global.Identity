using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Resource;
using VianaHub.Global.Identity.Application.Dto.Response.Resource;
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

public class ResourceEndpointTests : IClassFixture<ResourceEndpointTests.ResourceWebApplicationFactory>
{
    private readonly ResourceWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ResourceEndpointTests(ResourceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.NotifyMock.Reset();
        _factory.NotifyMock.Setup(x => x.HasNotify()).Returns(false);
        _factory.NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
        _factory.NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

        _factory.ResourceAppServiceMock.Reset();
    }

    #region GetAll

    [Fact(DisplayName = "GET /v1/resources - Deve retornar 200 com lista de resources")]
    [Trait("Api", "")]
    public async Task GetAll_Sucesso_DeveRetornar200()
    {
        var resources = Builder<ResourceResponse>.CreateListOfSize(3)
            .All()
            .With(x => x.IsActive = true)
            .Build()
            .ToList();

        _factory.ResourceAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(resources);

        var response = await _client.GetAsync("/v1/resources/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/resources - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetAll_ListaVazia_DeveRetornar200()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/v1/resources/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/resources - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetAll_Erro_DeveRetornar500()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/resources/");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetById

    [Fact(DisplayName = "GET /v1/resources/{id} - Deve retornar 200 quando resource existe")]
    [Trait("Api", "")]
    public async Task GetById_Sucesso_DeveRetornar200()
    {
        var resource = Builder<ResourceResponse>.CreateNew()
            .With(x => x.Id = 1)
            .With(x => x.IsActive = true)
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        var response = await _client.GetAsync("/v1/resources/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/resources/{id} - Deve retornar 410 quando resource não existe")]
    [Trait("Api", "")]
    public async Task GetById_NaoEncontrado_DeveRetornar410()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Resource não encontrado"]);

        var response = await _client.GetAsync("/v1/resources/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/resources/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetById_Erro_DeveRetornar500()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/resources/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetPaged

    [Fact(DisplayName = "GET /v1/resources/paged - Deve retornar 200 com lista paginada")]
    [Trait("Api", "")]
    public async Task GetPaged_Sucesso_DeveRetornar200()
    {
        var items = Builder<ResourceResponse>.CreateListOfSize(2)
            .All()
            .With(x => x.IsActive = true)
            .Build()
            .ToList();

        var paged = new ListPageResponse<ResourceResponse>(items, 1, 10, 2, 1);

        _factory.ResourceAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PagedFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var response = await _client.GetAsync("/v1/resources/paged?PageNumber=1&PageSize=10&Search=&SortBy=Name&SortDirection=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/resources/paged - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetPaged_ListaVazia_DeveRetornar200()
    {
        var paged = new ListPageResponse<ResourceResponse>([], 1, 10, 0, 0);

        _factory.ResourceAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PagedFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var response = await _client.GetAsync("/v1/resources/paged?PageNumber=1&PageSize=10&Search=&SortBy=Name&SortDirection=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/resources/paged - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetPaged_Erro_DeveRetornar500()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<PagedFilterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/resources/paged?PageNumber=1&PageSize=10&Search=&SortBy=Name&SortDirection=asc");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact(DisplayName = "POST /v1/resources - Deve retornar 201 quando resource criado com sucesso")]
    [Trait("Api", "")]
    public async Task Create_Sucesso_DeveRetornar201()
    {
        var request = Builder<CreateResourceRequest>.CreateNew()
            .With(x => x.AppId = 1)
            .With(x => x.Name = "Resource Test")
            .With(x => x.Description = "Descrição")
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PostAsJsonAsync("/v1/resources/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/resources - Deve retornar 400 quando serviço retorna falso com notificação")]
    [Trait("Api", "")]
    public async Task Create_DadosInvalidos_DeveRetornar400()
    {
        var request = Builder<CreateResourceRequest>.CreateNew()
            .With(x => x.AppId = 0)
            .With(x => x.Name = string.Empty)
            .With(x => x.Description = string.Empty)
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Nome é obrigatório"]);

        var response = await _client.PostAsJsonAsync("/v1/resources/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/resources - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Create_Erro_DeveRetornar500()
    {
        var request = Builder<CreateResourceRequest>.CreateNew()
            .With(x => x.AppId = 1)
            .With(x => x.Name = "Resource Test")
            .With(x => x.Description = "Descrição")
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateResourceRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PostAsJsonAsync("/v1/resources/", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Update

    [Fact(DisplayName = "PUT /v1/resources/{id} - Deve retornar 200 quando atualizado com sucesso")]
    [Trait("Api", "")]
    public async Task Update_Sucesso_DeveRetornar200()
    {
        var request = Builder<UpdateResourceRequest>.CreateNew()
            .With(x => x.Name = "Resource Atualizado")
            .With(x => x.Description = "Descrição atualizada")
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PutAsJsonAsync("/v1/resources/1", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/resources/{id} - Deve retornar 410 quando resource não existe")]
    [Trait("Api", "")]
    public async Task Update_NaoEncontrado_DeveRetornar410()
    {
        var request = Builder<UpdateResourceRequest>.CreateNew()
            .With(x => x.Name = "Resource")
            .With(x => x.Description = "Desc")
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.UpdateAsync(99, It.IsAny<UpdateResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Resource não encontrado"]);

        var response = await _client.PutAsJsonAsync("/v1/resources/99", request);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/resources/{id} - Deve retornar 409 quando nome já está em uso")]
    [Trait("Api", "")]
    public async Task Update_Duplicado_DeveRetornar409()
    {
        var request = Builder<UpdateResourceRequest>.CreateNew()
            .With(x => x.Name = "Resource Duplicado")
            .With(x => x.Description = "Desc")
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Conflict);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Nome de resource já está em uso"]);

        var response = await _client.PutAsJsonAsync("/v1/resources/1", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/resources/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Update_Erro_DeveRetornar500()
    {
        var request = Builder<UpdateResourceRequest>.CreateNew()
            .With(x => x.Name = "Resource")
            .With(x => x.Description = "Desc")
            .Build();

        _factory.ResourceAppServiceMock
            .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateResourceRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PutAsJsonAsync("/v1/resources/1", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Activate

    [Fact(DisplayName = "PATCH /v1/resources/{id}/activate - Deve retornar 200 quando ativado com sucesso")]
    [Trait("Api", "")]
    public async Task Activate_Sucesso_DeveRetornar200()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.ActivateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PatchAsync("/v1/resources/1/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/resources/{id}/activate - Deve retornar 410 quando resource não existe")]
    [Trait("Api", "")]
    public async Task Activate_NaoEncontrado_DeveRetornar410()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.ActivateAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Resource não encontrado"]);

        var response = await _client.PatchAsync("/v1/resources/99/activate", null);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/resources/{id}/activate - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Activate_Erro_DeveRetornar500()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.ActivateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PatchAsync("/v1/resources/1/activate", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Deactivate

    [Fact(DisplayName = "PATCH /v1/resources/{id}/deactivate - Deve retornar 200 quando desativado com sucesso")]
    [Trait("Api", "")]
    public async Task Deactivate_Sucesso_DeveRetornar200()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.DeactivateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PatchAsync("/v1/resources/1/deactivate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/resources/{id}/deactivate - Deve retornar 410 quando resource não existe")]
    [Trait("Api", "")]
    public async Task Deactivate_NaoEncontrado_DeveRetornar410()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.DeactivateAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Resource não encontrado"]);

        var response = await _client.PatchAsync("/v1/resources/99/deactivate", null);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/resources/{id}/deactivate - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Deactivate_Erro_DeveRetornar500()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.DeactivateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PatchAsync("/v1/resources/1/deactivate", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Delete

    [Fact(DisplayName = "DELETE /v1/resources/{id} - Deve retornar 200 quando excluído com sucesso")]
    [Trait("Api", "")]
    public async Task Delete_Sucesso_DeveRetornar200()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.DeleteAsync("/v1/resources/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/resources/{id} - Deve retornar 410 quando resource não existe")]
    [Trait("Api", "")]
    public async Task Delete_NaoEncontrado_DeveRetornar410()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Resource não encontrado"]);

        var response = await _client.DeleteAsync("/v1/resources/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/resources/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Delete_Erro_DeveRetornar500()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.DeleteAsync("/v1/resources/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region BulkUpload

    [Fact(DisplayName = "POST /v1/resources/bulk-upload - Deve retornar 200 quando upload realizado com sucesso")]
    [Trait("Api", "")]
    public async Task BulkUpload_Sucesso_DeveRetornar200()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.BulkUploadAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("id,name\n1,Resource1"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "resources.csv");

        var response = await _client.PostAsync("/v1/resources/bulk-upload", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/resources/bulk-upload - Deve retornar 400 quando nenhum arquivo enviado")]
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

        var response = await _client.PostAsync("/v1/resources/bulk-upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/resources/bulk-upload - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task BulkUpload_Erro_DeveRetornar500()
    {
        _factory.ResourceAppServiceMock
            .Setup(x => x.BulkUploadAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("id,name\n1,Resource1"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "resources.csv");

        var response = await _client.PostAsync("/v1/resources/bulk-upload", content);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region WebApplicationFactory

    public class ResourceWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IResourceAppService> ResourceAppServiceMock { get; } = new();
        public Mock<INotify> NotifyMock { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IResourceAppService>();
                services.RemoveAll<INotify>();

                NotifyMock.Setup(x => x.HasNotify()).Returns(false);
                NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
                NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

                services.AddScoped(_ => ResourceAppServiceMock.Object);
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
                new Claim("permission", "resources:read"),
                new Claim("permission", "resources:create"),
                new Claim("permission", "resources:update"),
                new Claim("permission", "resources:activate"),
                new Claim("permission", "resources:deactivate"),
                new Claim("permission", "resource:deactivate"),
                new Claim("permission", "resources:delete"),
                new Claim("permission", "resources:bulkupload")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    #endregion
}
