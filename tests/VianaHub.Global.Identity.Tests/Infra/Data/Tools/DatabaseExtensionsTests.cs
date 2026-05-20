using VianaHub.Global.Identity.Infra.Data.Context;
using VianaHub.Global.Identity.Infra.Data.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Tools;

public class DatabaseExtensionsTests
{
    private static IdentityDbContext CreateInMemoryContext() =>
        new(new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Mock<IHost> CreateHostMock(IdentityDbContext context, IHostEnvironment? hostEnv = null)
    {
        var loggerMock = new Mock<ILogger<IdentityDbContext>>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ILogger<IdentityDbContext>)))
            .Returns(loggerMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IdentityDbContext)))
            .Returns(context);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IHostEnvironment)))
            .Returns(hostEnv!);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        var hostMock = new Mock<IHost>();
        hostMock.Setup(x => x.Services).Returns(rootServiceProviderMock.Object);

        return hostMock;
    }

    #region CanConnectAsync - Sucesso

    [Fact(DisplayName = "CanConnectAsync - Deve retornar true quando conexão é bem-sucedida")]
    [Trait("Infra.Data", "")]
    public async Task CanConnectAsync_ConexaoBemSucedida_DeveRetornarTrue()
    {
        using var context = CreateInMemoryContext();

        var result = await context.CanConnectAsync();

        Assert.True(result);
    }

    [Fact(DisplayName = "CanConnectAsync - Deve aceitar CancellationToken")]
    [Trait("Infra.Data", "")]
    public async Task CanConnectAsync_ComCancellationToken_DeveRetornarTrue()
    {
        using var context = CreateInMemoryContext();
        using var cts = new CancellationTokenSource();

        var result = await context.CanConnectAsync(cts.Token);

        Assert.True(result);
    }

    #endregion

    #region CanConnectAsync - Insucesso

    [Fact(DisplayName = "CanConnectAsync - Deve retornar false quando exceção é lançada")]
    [Trait("Infra.Data", "")]
    public async Task CanConnectAsync_ExcecaoLancada_DeveRetornarFalse()
    {
        // Contexto com provider inválido para forçar falha
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer("Server=invalid_host_that_does_not_exist;Database=X;User Id=sa;Password=x;Connect Timeout=1;TrustServerCertificate=True")
            .Options;
        using var context = new IdentityDbContext(options);

        var result = await context.CanConnectAsync();

        Assert.False(result);
    }

    [Fact(DisplayName = "CanConnectAsync - Deve retornar false quando CancellationToken já está cancelado")]
    [Trait("Infra.Data", "")]
    public async Task CanConnectAsync_CancellationTokenCancelado_DeveRetornarFalse()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer("Server=invalid_host_that_does_not_exist;Database=X;User Id=sa;Password=x;Connect Timeout=1;TrustServerCertificate=True")
            .Options;
        using var context = new IdentityDbContext(options);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await context.CanConnectAsync(cts.Token);

        Assert.False(result);
    }

    #endregion

    #region InitializeDatabaseAsync - Sucesso (applyMigrations = false)

    [Fact(DisplayName = "InitializeDatabaseAsync - Deve concluir sem exceção quando applyMigrations é false")]
    [Trait("Infra.Data", "")]
    public async Task InitializeDatabaseAsync_SemMigrations_DeveConcluirSemExcecao()
    {
        using var context = CreateInMemoryContext();
        var hostMock = CreateHostMock(context);

        var exception = await Record.ExceptionAsync(() =>
            hostMock.Object.InitializeDatabaseAsync(applyMigrations: false));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "InitializeDatabaseAsync - Deve retornar sem chamar migrations quando applyMigrations é false")]
    [Trait("Infra.Data", "")]
    public async Task InitializeDatabaseAsync_SemMigrations_NaoDeveChamarMigrations()
    {
        using var context = CreateInMemoryContext();

        var loggerMock = new Mock<ILogger<IdentityDbContext>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ILogger<IdentityDbContext>)))
            .Returns(loggerMock.Object);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        var hostMock = new Mock<IHost>();
        hostMock.Setup(x => x.Services).Returns(rootServiceProviderMock.Object);

        await hostMock.Object.InitializeDatabaseAsync(applyMigrations: false);

        // Quando applyMigrations=false, IdentityDbContext não deve ser resolvido
        serviceProviderMock.Verify(x => x.GetService(typeof(IdentityDbContext)), Times.Never);
    }

    #endregion

    #region InitializeDatabaseAsync - Sucesso (applyMigrations = true)

        [Fact(DisplayName = "InitializeDatabaseAsync - Deve relançar exceção quando banco não suporta operações relacionais")]
        [Trait("Infra.Data", "")]
        public async Task InitializeDatabaseAsync_ComMigrationsProviderNaoRelacional_DeveRelancarExcecao()
        {
            using var context = CreateInMemoryContext();
            var hostMock = CreateHostMock(context);

            // InMemory provider não suporta GetAppliedMigrationsAsync (API relacional)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                hostMock.Object.InitializeDatabaseAsync(applyMigrations: true));
        }

        #endregion

    #region InitializeDatabaseAsync - Insucesso

    [Fact(DisplayName = "InitializeDatabaseAsync - Deve relançar exceção quando MigrateDatabaseAsync falha")]
    [Trait("Infra.Data", "")]
    public async Task InitializeDatabaseAsync_MigrateFalha_DeveRelancarExcecao()
    {
        var loggerMock = new Mock<ILogger<IdentityDbContext>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ILogger<IdentityDbContext>)))
            .Returns(loggerMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IdentityDbContext)))
            .Throws(new InvalidOperationException("Contexto indisponível"));

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        var hostMock = new Mock<IHost>();
        hostMock.Setup(x => x.Services).Returns(rootServiceProviderMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            hostMock.Object.InitializeDatabaseAsync(applyMigrations: true));
    }

    #endregion

    #region MigrateDatabaseAsync - Insucesso (provider não relacional)

        [Fact(DisplayName = "MigrateDatabaseAsync - Deve lançar InvalidOperationException com provider InMemory (não relacional)")]
        [Trait("Infra.Data", "")]
        public async Task MigrateDatabaseAsync_ProviderNaoRelacional_DeveLancarInvalidOperationException()
        {
            using var context = CreateInMemoryContext();
            var hostMock = CreateHostMock(context);

            // InMemory provider não suporta GetAppliedMigrationsAsync (API relacional)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                hostMock.Object.MigrateDatabaseAsync());
        }

        [Fact(DisplayName = "MigrateDatabaseAsync - Deve lançar InvalidOperationException com IHostEnvironment nulo e provider InMemory")]
        [Trait("Infra.Data", "")]
        public async Task MigrateDatabaseAsync_HostEnvironmentNuloProviderNaoRelacional_DeveLancarInvalidOperationException()
        {
            using var context = CreateInMemoryContext();
            var hostMock = CreateHostMock(context, hostEnv: null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                hostMock.Object.MigrateDatabaseAsync());
        }

        [Fact(DisplayName = "MigrateDatabaseAsync - Deve lançar InvalidOperationException com IHostEnvironment sem script e provider InMemory")]
        [Trait("Infra.Data", "")]
        public async Task MigrateDatabaseAsync_DiretorioSemScriptProviderNaoRelacional_DeveLancarInvalidOperationException()
        {
            using var context = CreateInMemoryContext();

            var hostEnvMock = new Mock<IHostEnvironment>();
            hostEnvMock.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

            var hostMock = CreateHostMock(context, hostEnvMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                hostMock.Object.MigrateDatabaseAsync());
        }

        #endregion

    #region MigrateDatabaseAsync - Insucesso

    [Fact(DisplayName = "MigrateDatabaseAsync - Deve relançar exceção quando IdentityDbContext não pode ser resolvido")]
    [Trait("Infra.Data", "")]
    public async Task MigrateDatabaseAsync_ContextoNaoResolvido_DeveRelancarExcecao()
    {
        var loggerMock = new Mock<ILogger<IdentityDbContext>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ILogger<IdentityDbContext>)))
            .Returns(loggerMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IdentityDbContext)))
            .Throws(new InvalidOperationException("Contexto não registrado"));

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        var hostMock = new Mock<IHost>();
        hostMock.Setup(x => x.Services).Returns(rootServiceProviderMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            hostMock.Object.MigrateDatabaseAsync());
    }

    [Fact(DisplayName = "MigrateDatabaseAsync - Deve relançar exceção quando ILogger não pode ser resolvido")]
    [Trait("Infra.Data", "")]
    public async Task MigrateDatabaseAsync_LoggerNaoResolvido_DeveRelancarExcecao()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(ILogger<IdentityDbContext>)))
            .Throws(new InvalidOperationException("Logger não registrado"));

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

        var rootServiceProviderMock = new Mock<IServiceProvider>();
        rootServiceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        var hostMock = new Mock<IHost>();
        hostMock.Setup(x => x.Services).Returns(rootServiceProviderMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            hostMock.Object.MigrateDatabaseAsync());
    }

    #endregion
}
