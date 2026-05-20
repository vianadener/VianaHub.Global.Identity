using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class RefreshTokenMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "RefreshTokenMapping - Deve implementar IEntityTypeConfiguration<RefreshTokenEntity>")]
    [Trait("Infra.Data", "")]
    public void RefreshTokenMapping_DeveImplementarInterface()
    {
        var sut = new RefreshTokenMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<RefreshTokenEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "RefreshTokenMapping - Deve mapear para tabela 'RefreshTokens' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void RefreshTokenMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RefreshTokenEntity>(ctx);

        Assert.Equal("RefreshTokens", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "RefreshTokenMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void RefreshTokenMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RefreshTokenEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "RefreshTokenMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("AppId")]
    [InlineData("UserId")]
    [InlineData("TokenHash")]
    [InlineData("ExpiresAt")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void RefreshTokenMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<RefreshTokenEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "RefreshTokenMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("RevokedAt")]
    [InlineData("RevokedBy")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void RefreshTokenMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<RefreshTokenEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "RefreshTokenMapping - Deve ter índice único em TenantId + AppId + TokenHash")]
    [Trait("Infra.Data", "")]
    public void RefreshTokenMapping_DeveConterIndiceUnicoToken()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RefreshTokenEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "AppId")
                && i.Properties.Any(p => p.Name == "TokenHash"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "RefreshTokenMapping - Deve ter FK configurada para User")]
    [Trait("Infra.Data", "")]
    public void RefreshTokenMapping_DeveConterFkParaUser()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RefreshTokenEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(UserEntity));

        Assert.NotNull(fk);
    }

    [Fact(DisplayName = "RefreshTokenMapping - Deve ter FK configurada para App")]
    [Trait("Infra.Data", "")]
    public void RefreshTokenMapping_DeveConterFkParaApp()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<RefreshTokenEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(AppEntity));

        Assert.NotNull(fk);
    }

    #endregion
}
