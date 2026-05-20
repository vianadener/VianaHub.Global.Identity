using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class TenantMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "TenantMapping - Deve implementar IEntityTypeConfiguration<TenantEntity>")]
    [Trait("Infra.Data", "")]
    public void TenantMapping_DeveImplementarInterface()
    {
        var sut = new TenantMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<TenantEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "TenantMapping - Deve mapear para tabela 'Tenants' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void TenantMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<TenantEntity>(ctx);

        Assert.Equal("Tenants", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "TenantMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void TenantMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<TenantEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "TenantMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("Name")]
    [InlineData("Description")]
    [InlineData("Alias")]
    [InlineData("IsActive")]
    [InlineData("IsDeleted")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void TenantMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<TenantEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "TenantMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("UrlImage")]
    [InlineData("Settings")]
    [InlineData("Remarks")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void TenantMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<TenantEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region MaxLength

    [Theory(DisplayName = "TenantMapping - Propriedades de texto devem ter MaxLength configurado")]
    [Trait("Infra.Data", "")]
    [InlineData("Name", 200)]
    [InlineData("Description", 500)]
    [InlineData("Alias", 30)]
    [InlineData("Remarks", 1000)]
    public void TenantMapping_PropriedadesTexto_DevemTerMaxLength(string propertyName, int maxLength)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<TenantEntity>(ctx, propertyName);

        Assert.Equal(maxLength, prop.GetMaxLength());
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "TenantMapping - Deve ter FK configurada em Users")]
    [Trait("Infra.Data", "")]
    public void TenantMapping_DeveConterFkParaUsers()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<TenantEntity>(ctx);
        var nav = entity.GetNavigations().FirstOrDefault(n => n.Name == "Users");

        Assert.NotNull(nav);
    }

    #endregion
}
