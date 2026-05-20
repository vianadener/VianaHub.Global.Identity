using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class RolePermissionMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "RolePermissionMapping - Deve implementar IEntityTypeConfiguration<RolePermissionEntity>")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveImplementarInterface()
    {
        var sut = new RolePermissionMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<RolePermissionEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "RolePermissionMapping - Deve mapear para tabela 'RolePermissions' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RolePermissionEntity>(ctx);

        Assert.Equal("RolePermissions", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "RolePermissionMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RolePermissionEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "RolePermissionMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("AppId")]
    [InlineData("RoleId")]
    [InlineData("ResourceId")]
    [InlineData("ActionId")]
    public void RolePermissionMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<RolePermissionEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "RolePermissionMapping - Deve ter índice único em TenantId + AppId + RoleId + ResourceId + ActionId")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveConterIndiceUnico()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RolePermissionEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "RoleId")
                && i.Properties.Any(p => p.Name == "ResourceId")
                && i.Properties.Any(p => p.Name == "ActionId"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "RolePermissionMapping - Deve ter FK configurada para Tenant")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveConterFkParaTenant()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RolePermissionEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TenantEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "RolePermissionMapping - Deve ter FK configurada para Role")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveConterFkParaRole()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RolePermissionEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(RoleEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "RolePermissionMapping - Deve ter FK configurada para Resource")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveConterFkParaResource()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RolePermissionEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(ResourceEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "RolePermissionMapping - Deve ter FK configurada para Action")]
    [Trait("Infra.Data", "")]
    public void RolePermissionMapping_DeveConterFkParaAction()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RolePermissionEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(ActionEntity));

        Assert.NotNull(fk);
    }

    #endregion
}
