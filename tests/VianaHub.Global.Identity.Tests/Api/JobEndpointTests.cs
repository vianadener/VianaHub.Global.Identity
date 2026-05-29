using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Job;
using VianaHub.Global.Identity.Application.Dto.Response.Job;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
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

public class JobEndpointTests : IClassFixture<JobEndpointTests.JobWebApplicationFactory>
{
    private readonly JobWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JobEndpointTests(JobWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.NotifyMock.Reset();
        _factory.NotifyMock.Setup(x => x.HasNotify()).Returns(false);
        _factory.NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
        _factory.NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

        _factory.JobAppServiceMock.Reset();
    }

    #region GetAll

    [Fact(DisplayName = "GET /v1/job-definitions - Deve retornar 200 com lista de jobs")]
    [Trait("Api", "")]
    public async Task GetAll_Sucesso_DeveRetornar200()
    {
        var jobs = Enumerable.Range(1, 3)
            .Select(i => new JobResponse(i, "Cat", $"Job{i}", "* * * * *", 5, true))
            .ToList();

        _factory.JobAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobs);

        var response = await _client.GetAsync("/v1/job-definitions/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/job-definitions - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetAll_ListaVazia_DeveRetornar200()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/v1/job-definitions/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/job-definitions - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetAll_Erro_DeveRetornar500()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/job-definitions/");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetById

    [Fact(DisplayName = "GET /v1/job-definitions/{id} - Deve retornar 200 quando job existe")]
    [Trait("Api", "")]
    public async Task GetById_Sucesso_DeveRetornar200()
    {
        var job = new JobDetailResponse(1, 1, "Tenant", "Cat", "Job Test", "Desc", "Purpose", "Type", "Execute", "* * * * *", "GMT Standard Time", false, 5, 5, "default", 3, "Config", false, null, null, null, null, "OK", 1, true);

        _factory.JobAppServiceMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        var response = await _client.GetAsync("/v1/job-definitions/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/job-definitions/{id} - Deve retornar 410 quando job não existe")]
    [Trait("Api", "")]
    public async Task GetById_NaoEncontrado_DeveRetornar410()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Job não encontrado"]);

        var response = await _client.GetAsync("/v1/job-definitions/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/job-definitions/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetById_Erro_DeveRetornar500()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/job-definitions/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region GetPaged

    [Fact(DisplayName = "GET /v1/job-definitions/paged - Deve retornar 200 com lista paginada")]
    [Trait("Api", "")]
    public async Task GetPaged_Sucesso_DeveRetornar200()
    {
        var items = Enumerable.Range(1, 2)
            .Select(i => new JobResponse(i, "Cat", $"Job{i}", "* * * * *", 5, true))
            .ToList();

        var paged = new ListPageResponse<JobResponse>(items, 1, 10, 2, 1);

        _factory.JobAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<JobPagedFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var response = await _client.GetAsync("/v1/job-definitions/paged?PageNumber=1&PageSize=10&Search=&SortBy=JobName&SortDirection=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/job-definitions/paged - Deve retornar 200 com lista vazia")]
    [Trait("Api", "")]
    public async Task GetPaged_ListaVazia_DeveRetornar200()
    {
        var paged = new ListPageResponse<JobResponse>([], 1, 10, 0, 0);

        _factory.JobAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<JobPagedFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var response = await _client.GetAsync("/v1/job-definitions/paged?PageNumber=1&PageSize=10&Search=&SortBy=JobName&SortDirection=asc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/job-definitions/paged - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task GetPaged_Erro_DeveRetornar500()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.GetPagedAsync(It.IsAny<JobPagedFilter>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.GetAsync("/v1/job-definitions/paged?PageNumber=1&PageSize=10&Search=&SortBy=JobName&SortDirection=asc");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact(DisplayName = "POST /v1/job-definitions - Deve retornar 201 quando job criado com sucesso")]
    [Trait("Api", "")]
    public async Task Create_Sucesso_DeveRetornar201()
    {
        var request = new CreateJobRequest("Categoria", "MeuJob", "Descrição", "Propósito", "Tipo", "", "0 * * * *");

        var response = await _client.PostAsJsonAsync("/v1/job-definitions/", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/job-definitions - Deve retornar 400 quando serviço retorna falso com notificação")]
    [Trait("Api", "")]
    public async Task Create_DadosInvalidos_DeveRetornar400()
    {
        var request = new CreateJobRequest("", string.Empty, string.Empty, "", "", "", "");

        _factory.JobAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateJobRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Nome do job é obrigatório"]);

        var response = await _client.PostAsJsonAsync("/v1/job-definitions/", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/job-definitions - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Create_Erro_DeveRetornar500()
    {
        var request = new CreateJobRequest("Categoria", "MeuJob", "Descrição", "Propósito", "Tipo", "", "0 * * * *");

        _factory.JobAppServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateJobRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PostAsJsonAsync("/v1/job-definitions/", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Execute

    [Fact(DisplayName = "POST /v1/job-definitions/{id}/execute - Deve retornar 200 quando job executado com sucesso")]
    [Trait("Api", "")]
    public async Task Execute_Sucesso_DeveRetornar200()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.ExecuteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PostAsJsonAsync("/v1/job-definitions/1/execute", (object)null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/job-definitions/{id}/execute - Deve retornar 410 quando job não existe")]
    [Trait("Api", "")]
    public async Task Execute_NaoEncontrado_DeveRetornar410()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.ExecuteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Job não encontrado"]);

        var response = await _client.PostAsJsonAsync("/v1/job-definitions/99/execute", (object)null);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/job-definitions/{id}/execute - Deve retornar 400 quando job não pode ser executado")]
    [Trait("Api", "")]
    public async Task Execute_JobInativo_DeveRetornar400()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.ExecuteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Job inativo não pode ser executado"]);

        var response = await _client.PostAsJsonAsync("/v1/job-definitions/1/execute", (object)null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/job-definitions/{id}/execute - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Execute_Erro_DeveRetornar500()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.ExecuteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PostAsJsonAsync("/v1/job-definitions/1/execute", (object)null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Update

    [Fact(DisplayName = "PUT /v1/job-definitions/{id} - Deve retornar 200 quando atualizado com sucesso")]
    [Trait("Api", "")]
    public async Task Update_Sucesso_DeveRetornar200()
    {
        var request = new UpdateJobRequest("Descrição atualizada", "", "0 0 * * *", "GMT Standard Time", 5, 3, "default", 3, "", true);

        _factory.JobAppServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateJobRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PutAsJsonAsync("/v1/job-definitions/1", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/job-definitions/{id} - Deve retornar 410 quando job não existe")]
    [Trait("Api", "")]
    public async Task Update_NaoEncontrado_DeveRetornar410()
    {
        var request = new UpdateJobRequest("Descrição", "", "", "GMT Standard Time", 5, 5, "default", 3, "", true);

        _factory.JobAppServiceMock
            .Setup(x => x.UpdateAsync(99, It.IsAny<UpdateJobRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Job não encontrado"]);

        var response = await _client.PutAsJsonAsync("/v1/job-definitions/99", request);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /v1/job-definitions/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Update_Erro_DeveRetornar500()
    {
        var request = new UpdateJobRequest("Descrição", "", "", "GMT Standard Time", 5, 5, "default", 3, "", true);

        _factory.JobAppServiceMock
            .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateJobRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PutAsJsonAsync("/v1/job-definitions/1", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Activate

    [Fact(DisplayName = "PATCH /v1/job-definitions/{id}/activate - Deve retornar 200 quando ativado com sucesso")]
    [Trait("Api", "")]
    public async Task Activate_Sucesso_DeveRetornar200()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.ActivateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PatchAsync("/v1/job-definitions/1/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/job-definitions/{id}/activate - Deve retornar 410 quando job não existe")]
    [Trait("Api", "")]
    public async Task Activate_NaoEncontrado_DeveRetornar410()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.ActivateAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Job não encontrado"]);

        var response = await _client.PatchAsync("/v1/job-definitions/99/activate", null);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/job-definitions/{id}/activate - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Activate_Erro_DeveRetornar500()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.ActivateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PatchAsync("/v1/job-definitions/1/activate", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Deactivate

    [Fact(DisplayName = "PATCH /v1/job-definitions/{id}/deactivate - Deve retornar 200 quando desativado com sucesso")]
    [Trait("Api", "")]
    public async Task Deactivate_Sucesso_DeveRetornar200()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.DeactivateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.PatchAsync("/v1/job-definitions/1/deactivate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/job-definitions/{id}/deactivate - Deve retornar 410 quando job não existe")]
    [Trait("Api", "")]
    public async Task Deactivate_NaoEncontrado_DeveRetornar410()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.DeactivateAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Job não encontrado"]);

        var response = await _client.PatchAsync("/v1/job-definitions/99/deactivate", null);

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "PATCH /v1/job-definitions/{id}/deactivate - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Deactivate_Erro_DeveRetornar500()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.DeactivateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.PatchAsync("/v1/job-definitions/1/deactivate", null);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region Delete

    [Fact(DisplayName = "DELETE /v1/job-definitions/{id} - Deve retornar 200 quando excluído com sucesso")]
    [Trait("Api", "")]
    public async Task Delete_Sucesso_DeveRetornar200()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await _client.DeleteAsync("/v1/job-definitions/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/job-definitions/{id} - Deve retornar 410 quando job não existe")]
    [Trait("Api", "")]
    public async Task Delete_NaoEncontrado_DeveRetornar410()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Job não encontrado"]);

        var response = await _client.DeleteAsync("/v1/job-definitions/99");

        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /v1/job-definitions/{id} - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Delete_Erro_DeveRetornar500()
    {
        _factory.JobAppServiceMock
            .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var response = await _client.DeleteAsync("/v1/job-definitions/1");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    #endregion

    #region WebApplicationFactory

    public class JobWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IJobAppService> JobAppServiceMock { get; } = new();
        public Mock<INotify> NotifyMock { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IJobAppService>();
                services.RemoveAll<INotify>();

                NotifyMock.Setup(x => x.HasNotify()).Returns(false);
                NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
                NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

                services.AddScoped(_ => JobAppServiceMock.Object);
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
                new Claim("permission", "jobdefinitions:read"),
                new Claim("permission", "jobdefinitions:create"),
                new Claim("permission", "jobdefinitions:update"),
                new Claim("permission", "jobdefinitions:execute"),
                new Claim("permission", "jobdefinitions:activate"),
                new Claim("permission", "jobdefinitions:deactivate"),
                new Claim("permission", "jobdefinitions:delete")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    #endregion
}
