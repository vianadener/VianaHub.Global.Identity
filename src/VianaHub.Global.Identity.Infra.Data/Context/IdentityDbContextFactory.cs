using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VianaHub.Global.Identity.Infra.Data.Context;

/// <summary>
/// Factory usada exclusivamente pelo dotnet-ef CLI para criar o IdentityDbContext em tempo de design (migrations).
/// Não é utilizada em runtime — a configuração real ocorre em Program.cs via DependencyInjection.cs.
/// Lê a connection string de appsettings.json / appsettings.Development.json do projeto Api,
/// sem nenhuma credencial hardcoded.
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "VianaHub.Global.Identity.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
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
