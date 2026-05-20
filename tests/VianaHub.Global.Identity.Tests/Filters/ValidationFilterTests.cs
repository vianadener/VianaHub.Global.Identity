using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Api.Filters;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Filters;

public class ValidationFilterTests
{
    private readonly Mock<IValidator<FakeRequest>> _validatorMock;
    private readonly Mock<ILogger<ValidationFilter<FakeRequest>>> _loggerMock;
    private readonly Mock<INotify> _notifyMock;

    public ValidationFilterTests()
    {
        _validatorMock = new Mock<IValidator<FakeRequest>>();
        _loggerMock = new Mock<ILogger<ValidationFilter<FakeRequest>>>();
        _notifyMock = new Mock<INotify>();
    }

    private ValidationFilter<FakeRequest> CriarFilter(bool registrarValidator = true)
    {
        var services = new ServiceCollection();
        if (registrarValidator)
            services.AddSingleton(_validatorMock.Object);
        services.AddSingleton<INotify>(_notifyMock.Object);
        var sp = services.BuildServiceProvider();

        return new ValidationFilter<FakeRequest>(sp, _loggerMock.Object);
    }

    private static EndpointFilterInvocationContext CriarContext(object? argumento = null, IServiceProvider? sp = null)
    {
        var services = new ServiceCollection();
        var serviceProvider = sp ?? services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;

        var contextMock = new Mock<EndpointFilterInvocationContext>();
        var argumentos = argumento != null
            ? new List<object?> { argumento }
            : new List<object?>();

        contextMock.Setup(x => x.HttpContext).Returns(httpContext);
        contextMock.Setup(x => x.Arguments).Returns(argumentos);
        return contextMock.Object;
    }

    [Fact(DisplayName = "ValidationFilter - Deve chamar next quando não há validador registrado")]
    [Trait("Api", "")]
    public async Task InvokeAsync_SemValidador_DeveChamarNext()
    {
        var filter = CriarFilter(registrarValidator: false);
        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        var services = new ServiceCollection();
        services.AddSingleton<INotify>(_notifyMock.Object);
        var context = CriarContext(new FakeRequest(), services.BuildServiceProvider());

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "ValidationFilter - Deve chamar next quando request não encontrado nos argumentos")]
    [Trait("Api", "")]
    public async Task InvokeAsync_SemRequestNosArgumentos_DeveChamarNext()
    {
        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<FakeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var filter = CriarFilter();
        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        var services = new ServiceCollection();
        services.AddSingleton(_validatorMock.Object);
        services.AddSingleton<INotify>(_notifyMock.Object);
        var context = CriarContext(null, services.BuildServiceProvider());

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "ValidationFilter - Deve chamar next quando validação passa")]
    [Trait("Api", "")]
    public async Task InvokeAsync_ValidacaoPassou_DeveChamarNext()
    {
        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<FakeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var filter = CriarFilter();
        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        var services = new ServiceCollection();
        services.AddSingleton(_validatorMock.Object);
        services.AddSingleton<INotify>(_notifyMock.Object);
        var context = CriarContext(new FakeRequest(), services.BuildServiceProvider());

        await filter.InvokeAsync(context, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "ValidationFilter - Deve retornar BadRequest quando validação falha")]
    [Trait("Api", "")]
    public async Task InvokeAsync_ValidacaoFalhou_DeveRetornarBadRequest()
    {
        var erros = new List<ValidationFailure>
        {
            new("Nome", "Nome é obrigatório"),
            new("Descricao", "Descrição é obrigatória")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<FakeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(erros));

        var filter = CriarFilter();
        var services2 = new ServiceCollection();
        services2.AddSingleton(_validatorMock.Object);
        services2.AddSingleton<INotify>(_notifyMock.Object);
        var context2 = CriarContext(new FakeRequest(), services2.BuildServiceProvider());

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(context2, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "ValidationFilter - Deve adicionar erros ao INotify quando validação falha")]
    [Trait("Api", "")]
    public async Task InvokeAsync_ValidacaoFalhou_DeveAdicionarErrosAoNotify()
    {
        var erros = new List<ValidationFailure>
        {
            new("Nome", "Nome é obrigatório")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<FakeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(erros));

        var filter = CriarFilter();
        var services3 = new ServiceCollection();
        services3.AddSingleton(_validatorMock.Object);
        services3.AddSingleton<INotify>(_notifyMock.Object);
        var context3 = CriarContext(new FakeRequest(), services3.BuildServiceProvider());

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        await filter.InvokeAsync(context3, next);

        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
    }

    public class FakeRequest
    {
        public string Nome { get; set; } = "Test";
    }
}
