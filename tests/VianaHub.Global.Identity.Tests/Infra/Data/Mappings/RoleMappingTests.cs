using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class RoleMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "RoleMapping - Deve implementar IEntityTypeConfiguration<RoleEntity>")]
    [Trait("Infra.Data", "")]
    public void RoleMapping_DeveImplementarInterface()
    {
        var sut = new RoleMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<RoleEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "RoleMapping - Deve mapear para tabela 'Roles' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void RoleMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RoleEntity>(ctx);

        Assert.Equal("Roles", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "RoleMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void RoleMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RoleEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "RoleMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("AppId")]
    [InlineData("Name")]
    [InlineData("Description")]
    [InlineData("IsActive")]
    [InlineData("IsDeleted")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void RoleMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<RoleEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "RoleMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void RoleMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<RoleEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region MaxLength

    [Theory(DisplayName = "RoleMapping - Propriedades de texto devem ter MaxLength configurado")]
    [Trait("Infra.Data", "")]
    [InlineData("Name", 100)]
    [InlineData("Description", 500)]
    public void RoleMapping_PropriedadesTexto_DevemTerMaxLength(string propertyName, int maxLength)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<RoleEntity>(ctx, propertyName);

        Assert.Equal(maxLength, prop.GetMaxLength());
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "RoleMapping - Deve ter índice único em TenantId + AppId + Name")]
    [Trait("Infra.Data", "")]
    public void RoleMapping_DeveConterIndiceUnicoTenantAppName()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RoleEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "Name"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "RoleMapping - Deve ter FK configurada para Tenant")]
    [Trait("Infra.Data", "")]
    public void RoleMapping_DeveConterFkParaTenant()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RoleEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TenantEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "RoleMapping - Deve ter navegação para Permissions")]
    [Trait("Infra.Data", "")]
    public void RoleMapping_DeveConterNavegacaoParaPermissions()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RoleEntity>(ctx);
        var nav = entity.GetNavigations().FirstOrDefault(n => n.Name == "Permissions");

        Assert.NotNull(nav);
    }

    [Fact(DisplayName = "RoleMapping - Deve ter navegação para UserRoles")]
    [Trait("Infra.Data", "")]
    public void RoleMapping_DeveConterNavegacaoParaUserRoles()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RoleEntity>(ctx);
        var nav = entity.GetNavigations().FirstOrDefault(n => n.Name == "UserRoles");

        Assert.NotNull(nav);
    }

    #endregion
}
