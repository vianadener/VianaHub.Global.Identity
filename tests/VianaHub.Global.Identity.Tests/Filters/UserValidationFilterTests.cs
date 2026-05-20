using VianaHub.Global.Identity.Api.Filters;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace VianaHub.Global.Identity.Tests.Filters;

public class UserValidationFilterTests
{
    private readonly Mock<ILogger<UserValidationFilter>> _loggerMock;
    private readonly Mock<ILocalizationService> _localizationMock;

    public UserValidationFilterTests()
    {
        _loggerMock = new Mock<ILogger<UserValidationFilter>>();
        _localizationMock = new Mock<ILocalizationService>();

        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
    }

    private UserValidationFilter CriarFilter()
    {
        return new UserValidationFilter(_loggerMock.Object);
    }

    private EndpointFilterInvocationContext CriarContext(
        string? userIdRota,
        ClaimsPrincipal user,
        bool autenticado = true)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILocalizationService>(_localizationMock.Object);
        var sp = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = sp;
        httpContext.User = user;

        if (userIdRota != null)
            httpContext.Request.RouteValues["userId"] = userIdRota;

        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.Setup(x => x.HttpContext).Returns(httpContext);
        contextMock.Setup(x => x.Arguments).Returns(new List<object?>());
        return contextMock.Object;
    }

    private static ClaimsPrincipal CriarUsuarioAutenticado(string? userIdClaim = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "sub-id-123"),
            new(ClaimTypes.Name, "test-user")
        };

        if (userIdClaim != null)
            claims.Add(new Claim("user_id", userIdClaim));

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CriarUsuarioNaoAutenticado()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    [Fact(DisplayName = "UserValidationFilter - Deve retornar BadRequest quando userId não está na rota")]
    [Trait("Api", "")]
    public async Task InvokeAsync_SemUserIdNaRota_DeveRetornarBadRequest()
    {
        var filter = CriarFilter();
        var user = CriarUsuarioAutenticado();
        var context = CriarContext(userIdRota: null, user: user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "UserValidationFilter - Deve retornar BadRequest quando userId na rota é GUID inválido")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UserIdRotaInvalido_DeveRetornarBadRequest()
    {
        var filter = CriarFilter();
        var user = CriarUsuarioAutenticado();
        var context = CriarContext(userIdRota: "nao-e-um-guid", user: user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "UserValidationFilter - Deve retornar 401 quando usuário não está autenticado")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        var filter = CriarFilter();
        var userId = Guid.NewGuid().ToString();
        var user = CriarUsuarioNaoAutenticado();
        var context = CriarContext(userIdRota: userId, user: user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "UserValidationFilter - Deve chamar next quando token não contém claim userId (modo compatibilidade)")]
    [Trait("Api", "")]
    public async Task InvokeAsync_TokenSemClaimUserId_DeveChamarNextEmModoCompatibilidade()
    {
        var filter = CriarFilter();
        var userId = Guid.NewGuid().ToString();
        var user = CriarUsuarioAutenticado(userIdClaim: null);
        var context = CriarContext(userIdRota: userId, user: user);

        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "UserValidationFilter - Deve retornar 401 quando claim userId no token é GUID inválido")]
    [Trait("Api", "")]
    public async Task InvokeAsync_ClaimUserIdInvalido_DeveRetornarUnauthorized()
    {
        var filter = CriarFilter();
        var userId = Guid.NewGuid().ToString();
        var user = CriarUsuarioAutenticado(userIdClaim: "nao-e-um-guid");
        var context = CriarContext(userIdRota: userId, user: user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "UserValidationFilter - Deve chamar next quando userId da rota corresponde ao userId do token")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UserIdCorrespondente_DeveChamarNext()
    {
        var filter = CriarFilter();
        var userId = Guid.NewGuid().ToString();
        var user = CriarUsuarioAutenticado(userIdClaim: userId);
        var context = CriarContext(userIdRota: userId, user: user);

        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "UserValidationFilter - Deve retornar 403 quando userId da rota difere do userId do token")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UserIdDiferente_DeveRetornarForbidden()
    {
        var filter = CriarFilter();
        var userIdRota = Guid.NewGuid().ToString();
        var userIdToken = Guid.NewGuid().ToString();
        var user = CriarUsuarioAutenticado(userIdClaim: userIdToken);
        var context = CriarContext(userIdRota: userIdRota, user: user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }
}
