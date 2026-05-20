using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class ResourceMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "ResourceMapping - Deve implementar IEntityTypeConfiguration<ResourceEntity>")]
    [Trait("Infra.Data", "")]
    public void ResourceMapping_DeveImplementarInterface()
    {
        var sut = new ResourceMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<ResourceEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "ResourceMapping - Deve mapear para tabela 'Resources' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void ResourceMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ResourceEntity>(ctx);

        Assert.Equal("Resources", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "ResourceMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void ResourceMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ResourceEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "ResourceMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("AppId")]
    [InlineData("Name")]
    [InlineData("Description")]
    [InlineData("IsActive")]
    [InlineData("IsDeleted")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void ResourceMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<ResourceEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "ResourceMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void ResourceMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<ResourceEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region MaxLength

    [Theory(DisplayName = "ResourceMapping - Propriedades de texto devem ter MaxLength configurado")]
    [Trait("Infra.Data", "")]
    [InlineData("Name", 200)]
    [InlineData("Description", 500)]
    public void ResourceMapping_PropriedadesTexto_DevemTerMaxLength(string propertyName, int maxLength)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<ResourceEntity>(ctx, propertyName);

        Assert.Equal(maxLength, prop.GetMaxLength());
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "ResourceMapping - Deve ter índice único em TenantId + AppId + Name")]
    [Trait("Infra.Data", "")]
    public void ResourceMapping_DeveConterIndiceUnicoTenantAppName()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ResourceEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "Name"));

        Assert.NotNull(index);
    }

    [Fact(DisplayName = "ResourceMapping - Deve ter índice único em TenantId + AppId + Id")]
    [Trait("Infra.Data", "")]
    public void ResourceMapping_DeveConterIndiceUnicoTenantAppId()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ResourceEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "Id"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "ResourceMapping - Deve ter FK configurada para Tenant")]
    [Trait("Infra.Data", "")]
    public void ResourceMapping_DeveConterFkParaTenant()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ResourceEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TenantEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "ResourceMapping - Deve ter navegação para RolePermissions")]
    [Trait("Infra.Data", "")]
    public void ResourceMapping_DeveConterNavegacaoParaRolePermissions()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ResourceEntity>(ctx);
        var nav = entity.GetNavigations().FirstOrDefault(n => n.Name == "RolePermissions");

        Assert.NotNull(nav);
    }

    #endregion
}
