using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Context;

public class IdentityDbContextFactoryTests
{
    private const string ValidConnectionString =
        "Server=localhost;Database=IdentityDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True";

    private static IdentityDbContextFactory_WithInMemoryConfig CreateSut(string? connectionString) =>
        new(connectionString);

    #region Construtor

    [Fact(DisplayName = "IdentityDbContextFactory - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = new IdentityDbContextFactory();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "IdentityDbContextFactory - Deve implementar IDesignTimeDbContextFactory")]
    [Trait("Infra.Data", "")]
    public void IdentityDbContextFactory_DeveImplementarInterface()
    {
        var sut = new IdentityDbContextFactory();

        Assert.IsAssignableFrom<IDesignTimeDbContextFactory<IdentityDbContext>>(sut);
    }

    #endregion

    #region CreateDbContext - Sucesso

    [Fact(DisplayName = "CreateDbContext - Deve retornar IdentityDbContext quando connection string está presente")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ConnectionStringPresente_DeveRetornarContexto()
    {
        var sut = CreateSut(ValidConnectionString);

        using var context = sut.CreateDbContext([]);

        Assert.NotNull(context);
    }

    [Fact(DisplayName = "CreateDbContext - Deve retornar instância do tipo IdentityDbContext")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ConnectionStringPresente_DeveRetornarTipoCorreto()
    {
        var sut = CreateSut(ValidConnectionString);

        using var context = sut.CreateDbContext([]);

        Assert.IsType<IdentityDbContext>(context);
    }

    [Fact(DisplayName = "CreateDbContext - Deve aceitar args vazio sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ArgsVazio_NaoDeveLancarExcecao()
    {
        var sut = CreateSut(ValidConnectionString);

        var exception = Record.Exception(() => sut.CreateDbContext([]));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "CreateDbContext - Deve aceitar args com valores sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ArgsComValores_NaoDeveLancarExcecao()
    {
        var sut = CreateSut(ValidConnectionString);

        var exception = Record.Exception(() => sut.CreateDbContext(["--environment", "Development"]));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "CreateDbContext - Deve retornar contextos distintos em chamadas sucessivas")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ChamadasSucessivas_DeveRetornarInstanciasDistintas()
    {
        var sut = CreateSut(ValidConnectionString);

        using var context1 = sut.CreateDbContext([]);
        using var context2 = sut.CreateDbContext([]);

        Assert.NotSame(context1, context2);
    }

    [Fact(DisplayName = "CreateDbContext - Deve retornar contexto configurado com SQL Server")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ConnectionStringPresente_DeveConfigurarSqlServer()
    {
        var sut = CreateSut(ValidConnectionString);

        using var context = sut.CreateDbContext([]);

        Assert.NotNull(context.Database);
    }

    [Fact(DisplayName = "CreateDbContext - Deve aceitar connection string com TrustServerCertificate")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ConnectionStringComTrustServerCertificate_NaoDeveLancarExcecao()
    {
        const string cs = "Server=localhost;Database=IdentityDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True";
        var sut = CreateSut(cs);

        var exception = Record.Exception(() => sut.CreateDbContext([]));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "CreateDbContext - Deve aceitar connection string com Integrated Security")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ConnectionStringComIntegratedSecurity_NaoDeveLancarExcecao()
    {
        const string cs = "Server=localhost;Database=IdentityDb;Integrated Security=True;TrustServerCertificate=True";
        var sut = CreateSut(cs);

        var exception = Record.Exception(() => sut.CreateDbContext([]));

        Assert.Null(exception);
    }

    [Fact(DisplayName = "CreateDbContext - Contexto retornado deve herdar de DbContext")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ConnectionStringPresente_DeveRetornarDbContext()
    {
        var sut = CreateSut(ValidConnectionString);

        using var context = sut.CreateDbContext([]);

        Assert.IsAssignableFrom<DbContext>(context);
    }

    [Fact(DisplayName = "CreateDbContext - Deve dispor o contexto sem lançar exceção")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_ContextoCriado_DeveDisporSemExcecao()
    {
        var sut = CreateSut(ValidConnectionString);
        var context = sut.CreateDbContext([]);

        var exception = Record.Exception(() => context.Dispose());

        Assert.Null(exception);
    }

    #endregion

    #region CreateDbContext - Insucesso

    [Fact(DisplayName = "CreateDbContext - Deve lançar InvalidOperationException quando connection string está ausente")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_SemConnectionString_DeveLancarInvalidOperationException()
    {
        var sut = CreateSut(connectionString: null);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.CreateDbContext([]));

        Assert.Contains("DefaultConnection", exception.Message);
    }

    [Fact(DisplayName = "CreateDbContext - Mensagem de erro deve citar appsettings.Development.json")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_SemConnectionString_MensagemDeveCitarAppsettingsDevelopment()
    {
        var sut = CreateSut(connectionString: null);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.CreateDbContext([]));

        Assert.Contains("appsettings.Development.json", exception.Message);
    }

    [Fact(DisplayName = "CreateDbContext - Mensagem de erro deve citar variável de ambiente ConnectionStrings__DefaultConnection")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_SemConnectionString_MensagemDeveCitarVariavelDeAmbiente()
    {
        var sut = CreateSut(connectionString: null);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.CreateDbContext([]));

        Assert.Contains("ConnectionStrings__DefaultConnection", exception.Message);
    }

    [Fact(DisplayName = "CreateDbContext - Exceção deve ser do tipo InvalidOperationException e não derivado")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_SemConnectionString_ExcecaoDeveTerTipoExato()
    {
        var sut = CreateSut(connectionString: null);

        var exception = Assert.Throws<InvalidOperationException>(() => sut.CreateDbContext([]));

        Assert.Equal(typeof(InvalidOperationException), exception.GetType());
    }

    [Fact(DisplayName = "CreateDbContext - Deve lançar exceção também com args preenchidos quando connection string está ausente")]
    [Trait("Infra.Data", "")]
    public void CreateDbContext_SemConnectionStringComArgs_DeveLancarInvalidOperationException()
    {
        var sut = CreateSut(connectionString: null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.CreateDbContext(["--environment", "Production"]));

        Assert.NotNull(exception);
    }

    #endregion
}

/// <summary>
/// Subclasse de teste que substitui a leitura de arquivos por configuração em memória,
/// isolando os testes do sistema de arquivos real do projeto Api.
/// </summary>
internal class IdentityDbContextFactory_WithInMemoryConfig(string? connectionString)
    : IdentityDbContextFactory
{
    private readonly string? _connectionString = connectionString;

    public new IdentityDbContext CreateDbContext(string[] args)
    {
        var data = _connectionString is not null
            ? new Dictionary<string, string?> { { "ConnectionStrings:DefaultConnection", _connectionString } }
            : new Dictionary<string, string?>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(data)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não encontrada. " +
                "Verifique appsettings.Development.json no projeto Api ou defina a variável de ambiente ConnectionStrings__DefaultConnection.");

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new IdentityDbContext(optionsBuilder.Options);
    }
}
