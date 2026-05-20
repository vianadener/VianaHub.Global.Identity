using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class UserRoleMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "UserRoleMapping - Deve implementar IEntityTypeConfiguration<UserRoleEntity>")]
    [Trait("Infra.Data", "")]
    public void UserRoleMapping_DeveImplementarInterface()
    {
        var sut = new UserRoleMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<UserRoleEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "UserRoleMapping - Deve mapear para tabela 'UserRoles' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void UserRoleMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserRoleEntity>(ctx);

        Assert.Equal("UserRoles", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "UserRoleMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void UserRoleMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserRoleEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "UserRoleMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("AppId")]
    [InlineData("UserId")]
    [InlineData("RoleId")]
    public void UserRoleMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<UserRoleEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "UserRoleMapping - Deve ter índice único em TenantId + AppId + UserId + RoleId")]
    [Trait("Infra.Data", "")]
    public void UserRoleMapping_DeveConterIndiceUnico()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserRoleEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "UserId")
                && i.Properties.Any(p => p.Name == "RoleId"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "UserRoleMapping - Deve ter FK configurada para Tenant")]
    [Trait("Infra.Data", "")]
    public void UserRoleMapping_DeveConterFkParaTenant()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserRoleEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TenantEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "UserRoleMapping - Deve ter FK configurada para User")]
    [Trait("Infra.Data", "")]
    public void UserRoleMapping_DeveConterFkParaUser()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserRoleEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(UserEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "UserRoleMapping - Deve ter FK configurada para Role")]
    [Trait("Infra.Data", "")]
    public void UserRoleMapping_DeveConterFkParaRole()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserRoleEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(RoleEntity));

        Assert.NotNull(fk);
    }

    #endregion
}
