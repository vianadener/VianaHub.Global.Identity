using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Dto.Response.Auth;
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

public class AuthEndpointTests : IClassFixture<AuthEndpointTests.AuthWebApplicationFactory>
{
    private readonly AuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointTests(AuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        _factory.NotifyMock.Reset();
        _factory.NotifyMock.Setup(x => x.HasNotify()).Returns(false);
        _factory.NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
        _factory.NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

        _factory.AuthAppServiceMock.Reset();
        _factory.ForgotPasswordAppServiceMock.Reset();
    }

    #region Register

    [Fact(DisplayName = "POST /v1/auth/register - Deve retornar 200 quando registro realizado com sucesso")]
    [Trait("Api", "")]
    public async Task Register_Sucesso_DeveRetornar200()
    {
        var request = Builder<RegisterRequest>.CreateNew()
            .With(x => x.TenantId = 1)
            .With(x => x.Name = "Usuário Teste")
            .With(x => x.Secret = "SenhaForte@123")
            .With(x => x.UrlImage = "https://img.example.com/foto.png")
            .Build();

        var response = Builder<AuthDetailResponse>.CreateNew().Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/register - Deve retornar 400 quando dados inválidos")]
    [Trait("Api", "")]
    public async Task Register_DadosInvalidos_DeveRetornar400()
    {
        var request = Builder<RegisterRequest>.CreateNew()
            .With(x => x.TenantId = 0)
            .With(x => x.Name = string.Empty)
            .With(x => x.Secret = string.Empty)
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Nome é obrigatório", "Senha é obrigatória"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/register - Deve retornar 409 quando usuário já existe")]
    [Trait("Api", "")]
    public async Task Register_UsuarioDuplicado_DeveRetornar409()
    {
        var request = Builder<RegisterRequest>.CreateNew()
            .With(x => x.TenantId = 1)
            .With(x => x.Name = "Usuário Existente")
            .With(x => x.Secret = "SenhaForte@123")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Conflict);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Usuário já cadastrado"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/register - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Register_Erro_DeveRetornar500()
    {
        var request = Builder<RegisterRequest>.CreateNew()
            .With(x => x.TenantId = 1)
            .With(x => x.Name = "Usuário Teste")
            .With(x => x.Secret = "SenhaForte@123")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/register", request);

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
    }

    #endregion

    #region Login

    [Fact(DisplayName = "POST /v1/auth/login - Deve retornar 200 quando login realizado com sucesso")]
    [Trait("Api", "")]
    public async Task Login_Sucesso_DeveRetornar200()
    {
        var request = Builder<LoginRequest>.CreateNew()
            .With(x => x.LoginIdentifier = "usuario@teste.com")
            .With(x => x.Password = "SenhaForte@123")
            .Build();

        var response = Builder<AuthDetailResponse>.CreateNew().Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/login - Deve retornar 400 quando dados inválidos")]
    [Trait("Api", "")]
    public async Task Login_DadosInvalidos_DeveRetornar400()
    {
        var request = Builder<LoginRequest>.CreateNew()
            .With(x => x.LoginIdentifier = string.Empty)
            .With(x => x.Password = string.Empty)
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Login é obrigatório", "Senha é obrigatória"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/login - Deve retornar 401 quando credenciais inválidas")]
    [Trait("Api", "")]
    public async Task Login_CredenciaisInvalidas_DeveRetornar401()
    {
        var request = Builder<LoginRequest>.CreateNew()
            .With(x => x.LoginIdentifier = "usuario@teste.com")
            .With(x => x.Password = "SenhaErrada")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Unauthorized);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Credenciais inválidas"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/login - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Login_Erro_DeveRetornar500()
    {
        var request = Builder<LoginRequest>.CreateNew()
            .With(x => x.LoginIdentifier = "usuario@teste.com")
            .With(x => x.Password = "SenhaForte@123")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
    }

    #endregion

    #region Refresh

    [Fact(DisplayName = "POST /v1/auth/refresh - Deve retornar 200 quando token renovado com sucesso")]
    [Trait("Api", "")]
    public async Task Refresh_Sucesso_DeveRetornar200()
    {
        var request = Builder<RefreshRequest>.CreateNew()
            .With(x => x.TenantId = 1)
            .With(x => x.RefreshToken = "valid-refresh-token")
            .Build();

        var response = Builder<AuthDetailResponse>.CreateNew().Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/refresh - Deve retornar 400 quando dados inválidos")]
    [Trait("Api", "")]
    public async Task Refresh_DadosInvalidos_DeveRetornar400()
    {
        var request = Builder<RefreshRequest>.CreateNew()
            .With(x => x.TenantId = 0)
            .With(x => x.RefreshToken = string.Empty)
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["RefreshToken é obrigatório"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/refresh", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/refresh - Deve retornar 401 quando token inválido ou expirado")]
    [Trait("Api", "")]
    public async Task Refresh_TokenInvalido_DeveRetornar401()
    {
        var request = Builder<RefreshRequest>.CreateNew()
            .With(x => x.TenantId = 1)
            .With(x => x.RefreshToken = "expired-token")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthDetailResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Unauthorized);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Token inválido ou expirado"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/refresh", request);

        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/refresh - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Refresh_Erro_DeveRetornar500()
    {
        var request = Builder<RefreshRequest>.CreateNew()
            .With(x => x.TenantId = 1)
            .With(x => x.RefreshToken = "valid-refresh-token")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.RefreshAsync(It.IsAny<RefreshRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/refresh", request);

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
    }

    #endregion

    #region Logout

    [Fact(DisplayName = "POST /v1/auth/logout - Deve retornar 200 quando logout realizado com sucesso")]
    [Trait("Api", "")]
    public async Task Logout_Sucesso_DeveRetornar200()
    {
        var request = Builder<RevokeRequest>.CreateNew()
            .With(x => x.Reason = "Saída voluntária")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.LogoutAsync(It.IsAny<RevokeRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/logout", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/logout - Deve retornar 401 quando não autenticado")]
    [Trait("Api", "")]
    public async Task Logout_NaoAutenticado_DeveRetornar401()
    {
        var clientSemAuth = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var request = Builder<RevokeRequest>.CreateNew()
            .With(x => x.Reason = "Saída")
            .Build();

        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Unauthorized);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Não autorizado"]);

        var httpResponse = await clientSemAuth.PostAsJsonAsync("/v1/auth/logout", request);

        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/logout - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task Logout_Erro_DeveRetornar500()
    {
        var request = Builder<RevokeRequest>.CreateNew()
            .With(x => x.Reason = "Saída voluntária")
            .Build();

        _factory.AuthAppServiceMock
            .Setup(x => x.LogoutAsync(It.IsAny<RevokeRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/logout", request);

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
    }

    #endregion

    #region ForgotPassword

    [Fact(DisplayName = "POST /v1/auth/forgot-password - Deve retornar 200 quando solicitação enviada com sucesso")]
    [Trait("Api", "")]
    public async Task ForgotPassword_Sucesso_DeveRetornar200()
    {
        var request = Builder<ForgotPasswordRequest>.CreateNew()
            .With(x => x.LoginIdentifier = "usuario@teste.com")
            .Build();

        var response = Builder<ForgotPasswordResponse>.CreateNew().Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/forgot-password", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/forgot-password - Deve retornar 400 quando dados inválidos")]
    [Trait("Api", "")]
    public async Task ForgotPassword_DadosInvalidos_DeveRetornar400()
    {
        var request = Builder<ForgotPasswordRequest>.CreateNew()
            .With(x => x.LoginIdentifier = string.Empty)
            .Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ForgotPasswordResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["LoginIdentifier é obrigatório"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/forgot-password", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/forgot-password - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task ForgotPassword_Erro_DeveRetornar500()
    {
        var request = Builder<ForgotPasswordRequest>.CreateNew()
            .With(x => x.LoginIdentifier = "usuario@teste.com")
            .Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ForgotPasswordAsync(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/forgot-password", request);

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
    }

    #endregion

    #region ValidateResetToken

    [Fact(DisplayName = "GET /v1/auth/reset-password/validate - Deve retornar 200 quando token válido")]
    [Trait("Api", "")]
    public async Task ValidateResetToken_Sucesso_DeveRetornar200()
    {
        var response = Builder<ValidateResetTokenResponse>.CreateNew().Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ValidateResetTokenAsync(It.IsAny<ValidateResetTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var httpResponse = await _client.GetAsync("/v1/auth/reset-password/validate?Token=valid-token");

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/auth/reset-password/validate - Deve retornar 400 quando token inválido")]
    [Trait("Api", "")]
    public async Task ValidateResetToken_TokenInvalido_DeveRetornar400()
    {
        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ValidateResetTokenAsync(It.IsAny<ValidateResetTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ValidateResetTokenResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Token inválido"]);

        var httpResponse = await _client.GetAsync("/v1/auth/reset-password/validate?Token=invalid-token");

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "GET /v1/auth/reset-password/validate - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task ValidateResetToken_Erro_DeveRetornar500()
    {
        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ValidateResetTokenAsync(It.IsAny<ValidateResetTokenRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var httpResponse = await _client.GetAsync("/v1/auth/reset-password/validate?Token=valid-token");

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
    }

    #endregion

    #region ResetPassword

    [Fact(DisplayName = "POST /v1/auth/reset-password - Deve retornar 200 quando senha redefinida com sucesso")]
    [Trait("Api", "")]
    public async Task ResetPassword_Sucesso_DeveRetornar200()
    {
        var request = Builder<ResetPasswordRequest>.CreateNew()
            .With(x => x.Token = "valid-reset-token")
            .With(x => x.NewPassword = "NovaSenha@123")
            .With(x => x.ConfirmPassword = "NovaSenha@123")
            .Build();

        var response = Builder<ResetPasswordResponse>.CreateNew().Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/reset-password", request);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/reset-password - Deve retornar 400 quando dados inválidos")]
    [Trait("Api", "")]
    public async Task ResetPassword_DadosInvalidos_DeveRetornar400()
    {
        var request = Builder<ResetPasswordRequest>.CreateNew()
            .With(x => x.Token = string.Empty)
            .With(x => x.NewPassword = string.Empty)
            .With(x => x.ConfirmPassword = string.Empty)
            .Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResetPasswordResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.BadRequest);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Token é obrigatório", "Nova senha é obrigatória"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/reset-password", request);

        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/reset-password - Deve retornar 409 quando nova senha igual à anterior")]
    [Trait("Api", "")]
    public async Task ResetPassword_SenhaIgualAnterior_DeveRetornar409()
    {
        var request = Builder<ResetPasswordRequest>.CreateNew()
            .With(x => x.Token = "valid-reset-token")
            .With(x => x.NewPassword = "SenhaAnterior@123")
            .With(x => x.ConfirmPassword = "SenhaAnterior@123")
            .Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResetPasswordResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Conflict);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["A nova senha não pode ser igual à senha atual"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/reset-password", request);

        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/reset-password - Deve retornar 410 quando token expirado")]
    [Trait("Api", "")]
    public async Task ResetPassword_TokenExpirado_DeveRetornar410()
    {
        var request = Builder<ResetPasswordRequest>.CreateNew()
            .With(x => x.Token = "expired-reset-token")
            .With(x => x.NewPassword = "NovaSenha@123")
            .With(x => x.ConfirmPassword = "NovaSenha@123")
            .Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResetPasswordResponse)null);
        _factory.NotifyMock
            .Setup(x => x.HasNotify()).Returns(true);
        _factory.NotifyMock
            .Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.Gone);
        _factory.NotifyMock
            .Setup(x => x.GetErrorMessage()).Returns(["Token expirado ou inválido"]);

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/reset-password", request);

        Assert.Equal(HttpStatusCode.Gone, httpResponse.StatusCode);
    }

    [Fact(DisplayName = "POST /v1/auth/reset-password - Deve retornar 500 quando serviço lança exceção")]
    [Trait("Api", "")]
    public async Task ResetPassword_Erro_DeveRetornar500()
    {
        var request = Builder<ResetPasswordRequest>.CreateNew()
            .With(x => x.Token = "valid-reset-token")
            .With(x => x.NewPassword = "NovaSenha@123")
            .With(x => x.ConfirmPassword = "NovaSenha@123")
            .Build();

        _factory.ForgotPasswordAppServiceMock
            .Setup(x => x.ResetPasswordAsync(It.IsAny<ResetPasswordRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Erro interno"));

        var httpResponse = await _client.PostAsJsonAsync("/v1/auth/reset-password", request);

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponse.StatusCode);
    }

    #endregion

    #region WebApplicationFactory

    public class AuthWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IAuthAppService> AuthAppServiceMock { get; } = new();
        public Mock<IForgotPasswordAppService> ForgotPasswordAppServiceMock { get; } = new();
        public Mock<INotify> NotifyMock { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthAppService>();
                services.RemoveAll<IForgotPasswordAppService>();
                services.RemoveAll<INotify>();

                NotifyMock.Setup(x => x.HasNotify()).Returns(false);
                NotifyMock.Setup(x => x.GetStatusCode()).Returns(HttpStatusCode.OK);
                NotifyMock.Setup(x => x.GetErrorMessage()).Returns([]);

                services.AddScoped(_ => AuthAppServiceMock.Object);
                services.AddScoped(_ => ForgotPasswordAppServiceMock.Object);
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
                new Claim(ClaimTypes.Role, "admin")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    #endregion
}
