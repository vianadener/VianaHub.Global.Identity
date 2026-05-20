using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class UserMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "UserMapping - Deve implementar IEntityTypeConfiguration<UserEntity>")]
    [Trait("Infra.Data", "")]
    public void UserMapping_DeveImplementarInterface()
    {
        var sut = new UserMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<UserEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "UserMapping - Deve mapear para tabela 'Users' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void UserMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserEntity>(ctx);

        Assert.Equal("Users", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "UserMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void UserMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "UserMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("Name")]
    [InlineData("LoginIdentifier")]
    [InlineData("NormalizedLoginIdentifier")]
    [InlineData("PasswordHash")]
    [InlineData("IsActive")]
    [InlineData("IsDeleted")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void UserMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<UserEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "UserMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("LastAccessAt")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void UserMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<UserEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region MaxLength

    [Theory(DisplayName = "UserMapping - Propriedades de texto devem ter MaxLength configurado")]
    [Trait("Infra.Data", "")]
    [InlineData("Name", 150)]
    [InlineData("LoginIdentifier", 500)]
    [InlineData("NormalizedLoginIdentifier", 500)]
    [InlineData("PasswordHash", 500)]
    public void UserMapping_PropriedadesTexto_DevemTerMaxLength(string propertyName, int maxLength)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<UserEntity>(ctx, propertyName);

        Assert.Equal(maxLength, prop.GetMaxLength());
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "UserMapping - Deve ter índice único em TenantId + NormalizedLoginIdentifier")]
    [Trait("Infra.Data", "")]
    public void UserMapping_DeveConterIndiceUnicoTenantNormalizedLogin()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "NormalizedLoginIdentifier"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "UserMapping - Deve ter FK configurada para Tenant")]
    [Trait("Infra.Data", "")]
    public void UserMapping_DeveConterFkParaTenant()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TenantEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "UserMapping - Deve ter navegação para UserRoles")]
    [Trait("Infra.Data", "")]
    public void UserMapping_DeveConterNavegacaoParaUserRoles()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<UserEntity>(ctx);
        var nav = entity.GetNavigations().FirstOrDefault(n => n.Name == "UserRoles");

        Assert.NotNull(nav);
    }

    #endregion
}
