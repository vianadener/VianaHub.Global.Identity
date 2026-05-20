using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class UserRoleDataRepositoryTests
{
    private const int TenantId = 1;
    private const int AppId = 1;
    private const int UserId = 10;
    private const int RoleId = 20;

    private UserRoleDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private UserRoleDataRepository CreateSutWithData(params UserRoleEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        SeedRelatedEntities(context);
        context.UserRoles.AddRange(entities);
        context.SaveChanges();
        return new UserRoleDataRepository(context);
    }

    private static void SeedRelatedEntities(VianaHub.Global.Identity.Infra.Data.Context.IdentityDbContext context)
    {
        context.Tenants.Add(RepositoryTestHelper.BuildTenant(TenantId));
        context.Apps.Add(RepositoryTestHelper.BuildApp(AppId, TenantId));
        context.Users.Add(RepositoryTestHelper.BuildUser(UserId, TenantId, "user10@test.com"));
        context.Users.Add(RepositoryTestHelper.BuildUser(UserId + 1, TenantId, "user11@test.com"));
        context.Roles.Add(RepositoryTestHelper.BuildRole(RoleId, TenantId, AppId, "Role 20"));
        context.Roles.Add(RepositoryTestHelper.BuildRole(RoleId + 1, TenantId, AppId, "Role 21"));
        context.SaveChanges();
    }

    #region Construtor

    [Fact(DisplayName = "UserRoleDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "UserRoleDataRepository - Deve implementar IUserRoleDataRepository")]
    [Trait("Infra.Data", "")]
    public void UserRoleDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IUserRoleDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de userRoles do tenant e app")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            new UserRoleEntity(TenantId, AppId, UserId, RoleId),
            new UserRoleEntity(TenantId, AppId, UserId + 1, RoleId));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há userRoles")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar userRoles de outro tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_OutroTenant_NaoDeveRetornar()
    {
        var sut = CreateSutWithData(new UserRoleEntity(99, AppId, UserId, RoleId));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar userRole quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrado_DeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildUserRole(1, TenantId, AppId, UserId, RoleId);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByUserIdAsync

    [Fact(DisplayName = "GetByUserIdAsync - Deve retornar userRoles do usuário")]
    [Trait("Infra.Data", "")]
    public async Task GetByUserIdAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            new UserRoleEntity(TenantId, AppId, UserId, RoleId),
            new UserRoleEntity(TenantId, AppId, UserId, RoleId + 1));

        var result = await sut.GetByUserIdAsync(UserId, default);

        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "GetByUserIdAsync - Deve retornar lista vazia quando não há userRoles")]
    [Trait("Infra.Data", "")]
    public async Task GetByUserIdAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetByUserIdAsync(UserId, default);

        Assert.Empty(result);
    }

    #endregion

    #region ExistsAsync

    [Fact(DisplayName = "ExistsAsync - Deve retornar true quando userRole existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(new UserRoleEntity(TenantId, AppId, UserId, RoleId));

        var result = await sut.ExistsAsync(TenantId, AppId, UserId, RoleId, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsAsync - Deve retornar false quando userRole não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsAsync(TenantId, AppId, UserId, RoleId, default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir userRole com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new UserRoleEntity(TenantId, AppId, UserId, RoleId);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir userRole com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new UserRoleEntity(TenantId, AppId, UserId, RoleId);
        context.UserRoles.Add(entity);
        await context.SaveChangesAsync();
        var sut = new UserRoleDataRepository(context);

        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
