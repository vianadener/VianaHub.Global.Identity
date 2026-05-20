using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class JwtKeyMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "JwtKeyMapping - Deve implementar IEntityTypeConfiguration<JwtKeyEntity>")]
    [Trait("Infra.Data", "")]
    public void JwtKeyMapping_DeveImplementarInterface()
    {
        var sut = new JwtKeyMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<JwtKeyEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "JwtKeyMapping - Deve mapear para tabela 'JwtKeys' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void JwtKeyMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<JwtKeyEntity>(ctx);

        Assert.Equal("JwtKeys", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "JwtKeyMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void JwtKeyMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<JwtKeyEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "JwtKeyMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("TenantId")]
    [InlineData("KeyId")]
    [InlineData("PublicKey")]
    [InlineData("PrivateKeyEncrypted")]
    [InlineData("Algorithm")]
    [InlineData("KeySize")]
    [InlineData("KeyType")]
    [InlineData("UsageCount")]
    [InlineData("ExpiresAt")]
    [InlineData("NextRotationAt")]
    [InlineData("ValidationCount")]
    [InlineData("RotationPolicyDays")]
    [InlineData("OverlapPeriodDays")]
    [InlineData("MaxTokenLifetimeMinutes")]
    [InlineData("IsActive")]
    [InlineData("IsDeleted")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void JwtKeyMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<JwtKeyEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "JwtKeyMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("ActivatedAt")]
    [InlineData("LastUsedAt")]
    [InlineData("RevokedAt")]
    [InlineData("LastValidatedAt")]
    [InlineData("RevokedReason")]
    [InlineData("ModifiedBy")]
    [InlineData("ModifiedAt")]
    public void JwtKeyMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<JwtKeyEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region MaxLength

    [Theory(DisplayName = "JwtKeyMapping - Propriedades de texto devem ter MaxLength configurado")]
    [Trait("Infra.Data", "")]
    [InlineData("Algorithm", 50)]
    [InlineData("KeyType", 50)]
    [InlineData("RevokedReason", 500)]
    public void JwtKeyMapping_PropriedadesTexto_DevemTerMaxLength(string propertyName, int maxLength)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<JwtKeyEntity>(ctx, propertyName);

        Assert.Equal(maxLength, prop.GetMaxLength());
    }

    #endregion

    #region Propriedade ignorada

    [Fact(DisplayName = "JwtKeyMapping - PlainPrivateKey deve ser ignorada (não persistida)")]
    [Trait("Infra.Data", "")]
    public void JwtKeyMapping_PlainPrivateKey_DeveSerIgnorada()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<JwtKeyEntity>(ctx);
        var prop = entity.FindProperty("PlainPrivateKey");

        Assert.Null(prop);
    }

    #endregion

    #region Relacionamentos

    [Fact(DisplayName = "JwtKeyMapping - Deve ter FK configurada para Tenant")]
    [Trait("Infra.Data", "")]
    public void JwtKeyMapping_DeveConterFkParaTenant()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<JwtKeyEntity>(ctx);
        var fk = entity.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TenantEntity));

        Assert.NotNull(fk);
    }

    #endregion
}
