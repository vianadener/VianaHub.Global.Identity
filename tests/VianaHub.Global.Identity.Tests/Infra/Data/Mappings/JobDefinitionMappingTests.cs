using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Mappings;

public class JobDefinitionMappingTests
{
    #region IEntityTypeConfiguration

    [Fact(DisplayName = "JobDefinitionMapping - Deve implementar IEntityTypeConfiguration<JobDefinitionEntity>")]
    [Trait("Infra.Data", "")]
    public void JobDefinitionMapping_DeveImplementarInterface()
    {
        var sut = new JobDefinitionMapping();

        Assert.IsAssignableFrom<IEntityTypeConfiguration<JobDefinitionEntity>>(sut);
    }

    #endregion

    #region Tabela e Schema

    [Fact(DisplayName = "JobDefinitionMapping - Deve mapear para tabela 'JobDefinitions' no schema 'dbo'")]
    [Trait("Infra.Data", "")]
    public void JobDefinitionMapping_DeveMappearTabelaCorreta()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<JobDefinitionEntity>(ctx);

        Assert.Equal("JobDefinitions", entity.GetTableName());
        Assert.Equal("dbo", entity.GetSchema());
    }

    #endregion

    #region Chave Primária

    [Fact(DisplayName = "JobDefinitionMapping - Deve ter chave primária na propriedade Id")]
    [Trait("Infra.Data", "")]
    public void JobDefinitionMapping_DeveConterChavePrimaria()
    {
        using var ctx = MappingTestHelper.CreateContext();
        var entity = MappingTestHelper.GetEntityType<JobDefinitionEntity>(ctx);
        var pk = entity.FindPrimaryKey();

        Assert.NotNull(pk);
        Assert.Contains(pk!.Properties, p => p.Name == "Id");
    }

    #endregion

    #region Propriedades obrigatórias

    [Theory(DisplayName = "JobDefinitionMapping - Propriedades obrigatórias não devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("Category")]
    [InlineData("Type")]
    [InlineData("Name")]
    [InlineData("ExecuteOnlyOnce")]
    [InlineData("TimeoutMinutes")]
    [InlineData("Priority")]
    [InlineData("MaxRetries")]
    [InlineData("IsSystemJob")]
    [InlineData("IsActive")]
    [InlineData("IsDeleted")]
    [InlineData("AddedBy")]
    [InlineData("AddedOn")]
    public void JobDefinitionMapping_PropriedadesObrigatorias_NaoDevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<JobDefinitionEntity>(ctx, propertyName);

        Assert.False(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser obrigatória.");
    }

    #endregion

    #region Propriedades opcionais

    [Theory(DisplayName = "JobDefinitionMapping - Propriedades opcionais devem ser nullable")]
    [Trait("Infra.Data", "")]
    [InlineData("HangfireJobId")]
    [InlineData("Description")]
    [InlineData("Purpose")]
    [InlineData("CronExpression")]
    [InlineData("Configuration")]
    [InlineData("Method")]
    [InlineData("TimeZoneId")]
    [InlineData("Queue")]
    [InlineData("LastRegisteredAt")]
    [InlineData("ModifiedAt")]
    public void JobDefinitionMapping_PropriedadesOpcionais_DevemSerNullable(string propertyName)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<JobDefinitionEntity>(ctx, propertyName);

        Assert.True(prop.IsNullable, $"A propriedade '{propertyName}' deveria ser opcional.");
    }

    #endregion

    #region MaxLength

    [Theory(DisplayName = "JobDefinitionMapping - Propriedades de texto devem ter MaxLength configurado")]
    [Trait("Infra.Data", "")]
    [InlineData("HangfireJobId", 100)]
    [InlineData("Category", 100)]
    [InlineData("Type", 200)]
    [InlineData("Name", 200)]
    [InlineData("Description", 1000)]
    [InlineData("Purpose", 1000)]
    [InlineData("CronExpression", 200)]
    [InlineData("Method", 200)]
    [InlineData("TimeZoneId", 200)]
    [InlineData("Queue", 200)]
    public void JobDefinitionMapping_PropriedadesTexto_DevemTerMaxLength(string propertyName, int maxLength)
    {
        using var ctx = MappingTestHelper.CreateContext();
        var prop = MappingTestHelper.GetProperty<JobDefinitionEntity>(ctx, propertyName);

        Assert.Equal(maxLength, prop.GetMaxLength());
    }

    #endregion
}
