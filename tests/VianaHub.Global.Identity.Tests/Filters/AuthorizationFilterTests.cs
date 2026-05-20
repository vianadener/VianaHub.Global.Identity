using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Api.Filters;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace VianaHub.Global.Identity.Tests.Filters;

public class AuthorizationFilterTests
{
    private readonly Mock<ILogger<AuthorizationFilter>> _loggerMock;
    private readonly Mock<ILocalizationService> _localizationMock;
    private readonly Mock<INotify> _notifyMock;

    public AuthorizationFilterTests()
    {
        _loggerMock = new Mock<ILogger<AuthorizationFilter>>();
        _localizationMock = new Mock<ILocalizationService>();
        _notifyMock = new Mock<INotify>();

        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
    }

    private AuthorizationFilter CriarFilter(string allowedRoles = "", string resource = "", string action = "")
    {
        return new AuthorizationFilter(allowedRoles, resource, action, _loggerMock.Object, _localizationMock.Object);
    }

    private EndpointFilterInvocationContext CriarContext(ClaimsPrincipal user, IServiceProvider? sp = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_notifyMock.Object);
        services.AddSingleton<ILocalizationService>(_localizationMock.Object);
        var serviceProvider = sp ?? services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext();
        httpContext.User = user;
        httpContext.RequestServices = serviceProvider;

        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.Setup(x => x.HttpContext).Returns(httpContext);
        contextMock.Setup(x => x.Arguments).Returns(new List<object?>());
        return contextMock.Object;
    }

    private static ClaimsPrincipal CriarUsuarioAutenticado(
        string[] roles = null,
        string[] permissoes = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-id-123"),
            new(ClaimTypes.Name, "test-user")
        };

        foreach (var role in roles ?? [])
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach (var perm in permissoes ?? [])
            claims.Add(new Claim("permission", perm));

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CriarUsuarioNaoAutenticado()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve retornar 401 quando usuário não está autenticado")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        var filter = CriarFilter();
        var context = CriarContext(CriarUsuarioNaoAutenticado());

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve chamar next quando usuário autenticado sem restrições")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioAutenticadoSemRestricoes_DeveChamarNext()
    {
        var filter = CriarFilter(allowedRoles: "", resource: "", action: "");
        var user = CriarUsuarioAutenticado(["admin"]);
        var context = CriarContext(user);

        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve retornar 403 quando usuário não possui a role necessária")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioSemRole_DeveRetornarForbidden()
    {
        var filter = CriarFilter(allowedRoles: "admin");
        var user = CriarUsuarioAutenticado(["viewer"]);
        var context = CriarContext(user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve chamar next quando usuário possui a role necessária")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioComRoleCorreta_DeveChamarNext()
    {
        var filter = CriarFilter(allowedRoles: "admin");
        var user = CriarUsuarioAutenticado(["admin"]);
        var context = CriarContext(user);

        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve chamar next quando usuário possui uma das roles permitidas")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioComUmadasRolesPermitidas_DeveChamarNext()
    {
        var filter = CriarFilter(allowedRoles: "admin,manager,viewer");
        var user = CriarUsuarioAutenticado(["manager"]);
        var context = CriarContext(user);

        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve retornar 403 quando usuário não possui permissão para o recurso")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioSemPermissaoParaRecurso_DeveRetornarForbidden()
    {
        var filter = CriarFilter(allowedRoles: "admin", resource: "actions", action: "delete");
        var user = CriarUsuarioAutenticado(roles: ["admin"], permissoes: ["actions:read"]);
        var context = CriarContext(user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve chamar next quando usuário possui permissão para o recurso e ação")]
    [Trait("Api", "")]
    public async Task InvokeAsync_UsuarioComPermissaoParaRecursoEAcao_DeveChamarNext()
    {
        var filter = CriarFilter(allowedRoles: "admin", resource: "actions", action: "read");
        var user = CriarUsuarioAutenticado(roles: ["admin"], permissoes: ["actions:read"]);
        var context = CriarContext(user);

        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve adicionar notificação ao INotify quando role insuficiente")]
    [Trait("Api", "")]
    public async Task InvokeAsync_RoleInsuficiente_DeveAdicionarNotificacao()
    {
        var filter = CriarFilter(allowedRoles: "admin");
        var user = CriarUsuarioAutenticado(["viewer"]);
        var context = CriarContext(user);

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        await filter.InvokeAsync(context, next);

        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<int>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "AuthorizationFilter - Deve validar permissão no formato JSON 'permissions'")]
    [Trait("Api", "")]
    public async Task InvokeAsync_PermissaoFormatoJson_DeveChamarNext()
    {
        var filter = CriarFilter(allowedRoles: "admin", resource: "actions", action: "write");
        var permissionsJson = "{\"actions\":[\"read\",\"write\",\"delete\"]}";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-id-123"),
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.Role, "admin"),
            new("permissions", permissionsJson)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);
        var context = CriarContext(user);

        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }
}
