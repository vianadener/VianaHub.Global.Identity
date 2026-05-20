using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class ActionMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "ActionMapping - Deve implementar IEntityTypeConfiguration<ActionEntity>")]
    [Trait("Infra.Data", "")]
    public void ActionMapping_DeveImplementarInterface()
    {
        var sut = new ActionMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<ActionEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "ActionMapping - Deve mapear para tabela 'Actions' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void ActionMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ActionEntity>(ctx);

        Assert.Equal("Actions", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "ActionMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void ActionMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ActionEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "ActionMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("AppId")]
    [InlineData("Name")]
    [InlineData("Description")]
    [InlineData("IsActive")]
    [InlineData("IsDeleted")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void ActionMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<ActionEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "ActionMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void ActionMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<ActionEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region MaxLength

    [Theory(DisplayName = "ActionMapping - Propriedades de texto devem ter MaxLength configurado")]
    [Trait("Infra.Data", "")]
    [InlineData("Name", 50)]
    [InlineData("Description", 500)]
    public void ActionMapping_PropriedadesTexto_DevemTerMaxLength(string propertyName, int maxLength)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<ActionEntity>(ctx, propertyName);

        Assert.Equal(maxLength, prop.GetMaxLength());
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "ActionMapping - Deve ter índice único em TenantId + AppId + Name")]
    [Trait("Infra.Data", "")]
    public void ActionMapping_DeveConterIndiceUnicoTenantAppName()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ActionEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "Name"));

        Assert.NotNull(index);
    }

    [Fact(DisplayName = "ActionMapping - Deve ter índice único em TenantId + AppId + Id")]
    [Trait("Infra.Data", "")]
    public void ActionMapping_DeveConterIndiceUnicoTenantAppId()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ActionEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "Id"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "ActionMapping - Deve ter FK configurada para Tenant")]
    [Trait("Infra.Data", "")]
    public void ActionMapping_DeveConterFkParaTenant()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ActionEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TenantEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "ActionMapping - Deve ter navegação para RolePermissions")]
    [Trait("Infra.Data", "")]
    public void ActionMapping_DeveConterNavegacaoParaRolePermissions()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<ActionEntity>(ctx);
        var nav = entity.GetNavigations().FirstOrDefault(n => n.Name == "RolePermissions");

        Assert.NotNull(nav);
    }

    #endregion
}
