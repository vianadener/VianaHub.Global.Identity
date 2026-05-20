using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class PasswordResetTokenMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "PasswordResetTokenMapping - Deve implementar IEntityTypeConfiguration<PasswordResetTokenEntity>")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokenMapping_DeveImplementarInterface()
    {
        var sut = new PasswordResetTokenMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<PasswordResetTokenEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "PasswordResetTokenMapping - Deve mapear para tabela 'PasswordResetTokens' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokenMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<PasswordResetTokenEntity>(ctx);

        Assert.Equal("PasswordResetTokens", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "PasswordResetTokenMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokenMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<PasswordResetTokenEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "PasswordResetTokenMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("UserId")]
    [InlineData("TokenHash")]
    [InlineData("ExpiresAt")]
    [InlineData("Used")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void PasswordResetTokenMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<PasswordResetTokenEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "PasswordResetTokenMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void PasswordResetTokenMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<PasswordResetTokenEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region Índices

    [Fact(DisplayName = "PasswordResetTokenMapping - Deve ter índice único em TenantId + TokenHash")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokenMapping_DeveConterIndiceUnicoTenantToken()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<PasswordResetTokenEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.IsUnique
                && i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "TokenHash"));

        Assert.NotNull(index);
    }

    [Fact(DisplayName = "PasswordResetTokenMapping - Deve ter índice de rate limit em TenantId + UserId + AddedOn")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokenMapping_DeveConterIndiceRateLimit()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<PasswordResetTokenEntity>(ctx);
        var index = entity.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "TenantId")
                && i.Properties.Any(p => p.Name == "UserId")
                && i.Properties.Any(p => p.Name == "AddedOn"));

        Assert.NotNull(index);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "PasswordResetTokenMapping - Deve ter FK configurada para User")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokenMapping_DeveConterFkParaUser()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<PasswordResetTokenEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(UserEntity));

        Assert.NotNull(fk);
    }

    #endregion
}
