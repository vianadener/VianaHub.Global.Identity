using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Context;

public class IdentityDbContextTests
{
    private static IdentityDbContext CreateSut()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options);
    }

    #region Construtor

    [Fact(DisplayName = "IdentityDbContext - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciarContexto()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut);
    }

    #endregion

    #region DbSets

    [Fact(DisplayName = "Tenants - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void Tenants_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.Tenants);
    }

    [Fact(DisplayName = "Apps - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void Apps_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.Apps);
    }

    [Fact(DisplayName = "Users - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void Users_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.Users);
    }

    [Fact(DisplayName = "Roles - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void Roles_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.Roles);
    }

    [Fact(DisplayName = "Resources - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void Resources_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.Resources);
    }

    [Fact(DisplayName = "Actions - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void Actions_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.Actions);
    }

    [Fact(DisplayName = "RolePermissions - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void RolePermissions_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.RolePermissions);
    }

    [Fact(DisplayName = "UserRoles - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void UserRoles_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.UserRoles);
    }

    [Fact(DisplayName = "JwtKeys - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void JwtKeys_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.JwtKeys);
    }

    [Fact(DisplayName = "RefreshTokens - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void RefreshTokens_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.RefreshTokens);
    }

    [Fact(DisplayName = "PasswordResetTokens - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void PasswordResetTokens_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.PasswordResetTokens);
    }

    [Fact(DisplayName = "JobDefinitionEntities - DbSet deve estar configurado")]
    [Trait("Infra.Data", "")]
    public void JobDefinitionEntities_DeveEstarConfigurado()
    {
        using var sut = CreateSut();

        Assert.NotNull(sut.JobDefinitionEntities);
    }

    #endregion

    #region Persistência

    [Fact(DisplayName = "SaveChangesAsync - Deve persistir entidade no banco em memória")]
    [Trait("Infra.Data", "")]
    public async Task SaveChangesAsync_Sucesso_DevePersistirEntidade()
    {
        using var sut = CreateSut();
        var tenant = new TenantEntity("Tenant Teste", "Descrição Teste", "tenant-teste", null, null, null, 1);

        sut.Tenants.Add(tenant);
        var rows = await sut.SaveChangesAsync();

        Assert.Equal(1, rows);
        Assert.NotNull(await sut.Tenants.FirstOrDefaultAsync());
    }

    [Fact(DisplayName = "SaveChangesAsync - Deve retornar 0 quando não há alterações")]
    [Trait("Infra.Data", "")]
    public async Task SaveChangesAsync_SemAlteracoes_DeveRetornarZero()
    {
        using var sut = CreateSut();

        var rows = await sut.SaveChangesAsync();

        Assert.Equal(0, rows);
    }

    #endregion

    #region Schema

    [Fact(DisplayName = "OnModelCreating - Deve aplicar schema padrão 'dbo'")]
    [Trait("Infra.Data", "")]
    public void OnModelCreating_DeveAplicarSchemapadrao()
    {
        using var sut = CreateSut();
        var model = sut.Model;

        Assert.Equal("dbo", model.GetDefaultSchema());
    }

    #endregion
}
