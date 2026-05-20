using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Context;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

/// <summary>
/// Helper para criação de IdentityDbContext com banco InMemory nos testes de mapping.
/// </summary>
internal static class MappingTestHelper
{
    internal static IdentityDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options);
    }

    internal static Microsoft.EntityFrameworkCore.Metadata.IEntityType GetEntityType<T>(IdentityDbContext ctx)
        where T : class
        => ctx.Model.FindEntityType(typeof(T))!;

    internal static Microsoft.EntityFrameworkCore.Metadata.IProperty GetProperty<T>(
        IdentityDbContext ctx, string propertyName)
        where T : class
        => GetEntityType<T>(ctx).FindProperty(propertyName)!;
}
