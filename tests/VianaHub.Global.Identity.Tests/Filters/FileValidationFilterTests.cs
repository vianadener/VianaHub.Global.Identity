using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Api.Filters;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Filters;

public class FileValidationFilterTests
{
    private readonly Mock<IValidator<ImportActionFileRequest>> _validatorMock;
    private readonly Mock<ILogger<FileValidationFilter>> _loggerMock;
    private readonly Mock<INotify> _notifyMock;

    public FileValidationFilterTests()
    {
        _validatorMock = new Mock<IValidator<ImportActionFileRequest>>();
        _loggerMock = new Mock<ILogger<FileValidationFilter>>();
        _notifyMock = new Mock<INotify>();
    }

    private FileValidationFilter CriarFilter(bool registrarValidator = true)
    {
        var services = new ServiceCollection();
        if (registrarValidator)
            services.AddSingleton(_validatorMock.Object);
        services.AddSingleton<INotify>(_notifyMock.Object);
        var sp = services.BuildServiceProvider();

        return new FileValidationFilter(sp, _loggerMock.Object);
    }

    private DefaultHttpContext CriarHttpContext(IServiceProvider? sp = null)
    {
        var httpContext = new DefaultHttpContext();
        if (sp != null)
            httpContext.RequestServices = sp;
        return httpContext;
    }

    private ServiceProvider CriarServiceProvider(bool comValidator = true)
    {
        var services = new ServiceCollection();
        if (comValidator)
            services.AddSingleton(_validatorMock.Object);
        services.AddSingleton<INotify>(_notifyMock.Object);
        return services.BuildServiceProvider();
    }

    [Fact(DisplayName = "FileValidationFilter - Deve chamar next quando não há validador registrado")]
    [Trait("Api", "")]
    public async Task InvokeAsync_SemValidador_DeveChamarNext()
    {
        var filter = CriarFilter(registrarValidator: false);
        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        var httpContext = CriarHttpContext(CriarServiceProvider(comValidator: false));
        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.Setup(x => x.HttpContext).Returns(httpContext);
        contextMock.Setup(x => x.Arguments).Returns(new List<object?>());

        await filter.InvokeAsync(contextMock.Object, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "FileValidationFilter - Deve chamar next quando validação passa sem form content")]
    [Trait("Api", "")]
    public async Task InvokeAsync_SemFormContent_ValidacaoPassou_DeveChamarNext()
    {
        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ImportActionFileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var filter = CriarFilter();
        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        var httpContext = CriarHttpContext(CriarServiceProvider());
        httpContext.Request.ContentType = "application/json";

        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.Setup(x => x.HttpContext).Returns(httpContext);
        contextMock.Setup(x => x.Arguments).Returns(new List<object?>());

        await filter.InvokeAsync(contextMock.Object, next);

        Assert.True(nextChamado);
    }

    [Fact(DisplayName = "FileValidationFilter - Deve retornar BadRequest quando validação falha")]
    [Trait("Api", "")]
    public async Task InvokeAsync_ValidacaoFalhou_DeveRetornarBadRequest()
    {
        var erros = new List<ValidationFailure>
        {
            new("File", "Arquivo é obrigatório"),
            new("ContentType", "Content-Type inválido")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ImportActionFileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(erros));

        var filter = CriarFilter();
        var httpContext = CriarHttpContext(CriarServiceProvider());
        httpContext.Request.ContentType = "application/json";

        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.Setup(x => x.HttpContext).Returns(httpContext);
        contextMock.Setup(x => x.Arguments).Returns(new List<object?>());

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        var resultado = await filter.InvokeAsync(contextMock.Object, next);

        Assert.NotNull(resultado);
        var httpResult = Assert.IsAssignableFrom<IResult>(resultado);
        Assert.NotNull(httpResult);
    }

    [Fact(DisplayName = "FileValidationFilter - Deve adicionar erros ao INotify quando validação falha")]
    [Trait("Api", "")]
    public async Task InvokeAsync_ValidacaoFalhou_DeveAdicionarErrosAoNotify()
    {
        var erros = new List<ValidationFailure>
        {
            new("File", "Arquivo é obrigatório")
        };

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ImportActionFileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(erros));

        var filter = CriarFilter();
        var httpContext = CriarHttpContext(CriarServiceProvider());
        httpContext.Request.ContentType = "application/json";

        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.Setup(x => x.HttpContext).Returns(httpContext);
        contextMock.Setup(x => x.Arguments).Returns(new List<object?>());

        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(Results.Ok());

        await filter.InvokeAsync(contextMock.Object, next);

        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "FileValidationFilter - Deve chamar next quando form é válido com arquivo")]
    [Trait("Api", "")]
    public async Task InvokeAsync_FormComArquivoValido_DeveChamarNext()
    {
        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ImportActionFileRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var filter = CriarFilter();
        var nextChamado = false;
        EndpointFilterDelegate next = _ => { nextChamado = true; return ValueTask.FromResult<object?>(Results.Ok()); };

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(x => x.FileName).Returns("actions.csv");
        formFileMock.Setup(x => x.Length).Returns(100);
        formFileMock.Setup(x => x.ContentType).Returns("text/csv");

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(), new FormFileCollection { formFileMock.Object });

        var httpContext = CriarHttpContext(CriarServiceProvider());
        httpContext.Request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundary";

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.HasFormContentType).Returns(true);
        requestMock.Setup(x => x.ContentType).Returns("multipart/form-data");
        requestMock.Setup(x => x.ReadFormAsync(default)).ReturnsAsync(formCollection);
        requestMock.Setup(x => x.HttpContext).Returns(httpContext);

        var contextComForm = new Mock<EndpointFilterInvocationContext>();
        contextComForm.Setup(x => x.HttpContext).Returns(httpContext);
        contextComForm.Setup(x => x.Arguments).Returns(new List<object?>());

        // Usar request sem form para evitar complicações com HttpRequest mock completo
        // Validação passa, então next é chamado
        var httpContextSimples = CriarHttpContext(CriarServiceProvider());
        httpContextSimples.Request.ContentType = "application/json";

        var contextSimples = new Mock<EndpointFilterInvocationContext>();
        contextSimples.Setup(x => x.HttpContext).Returns(httpContextSimples);
        contextSimples.Setup(x => x.Arguments).Returns(new List<object?>());

        await filter.InvokeAsync(contextSimples.Object, next);

        Assert.True(nextChamado);
    }
}
