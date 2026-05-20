using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class RolePermissionDataRepositoryTests
{
    private const int TenantId = 1;
    private const int AppId = 1;
    private const int RoleId = 10;
    private const int ResourceId = 20;
    private const int ActionId = 30;

    private RolePermissionDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private RolePermissionDataRepository CreateSutWithData(params RolePermissionEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        SeedRelatedEntities(context);
        context.RolePermissions.AddRange(entities);
        context.SaveChanges();
        return new RolePermissionDataRepository(context);
    }

    private static void SeedRelatedEntities(VianaHub.Global.Identity.Infra.Data.Context.IdentityDbContext context)
    {
        context.Tenants.Add(RepositoryTestHelper.BuildTenant(TenantId));
        context.Apps.Add(RepositoryTestHelper.BuildApp(AppId, TenantId));
        context.Roles.Add(RepositoryTestHelper.BuildRole(RoleId, TenantId, AppId, "Role 10"));
        context.Resources.Add(RepositoryTestHelper.BuildResource(ResourceId, TenantId, AppId, "Resource 20"));
        context.Actions.Add(RepositoryTestHelper.BuildAction(ActionId, TenantId, AppId, "Action 30"));
        context.Actions.Add(RepositoryTestHelper.BuildAction(31, TenantId, AppId, "Action 31"));
        context.SaveChanges();
    }

    #region Construtor

    [Fact(DisplayName = "RolePermissionDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "RolePermissionDataRepository - Deve implementar IRolePermissionDataRepository")]
    [Trait("Infra.Data", "")]
    public void RolePermissionDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IRolePermissionDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de permissões do tenant e app")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, ActionId),
            new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, 31));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há permissões")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar permissões de outro tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_OutroTenant_NaoDeveRetornar()
    {
        var sut = CreateSutWithData(new RolePermissionEntity(99, AppId, RoleId, ResourceId, ActionId));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar permissão quando encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrada_DeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildRolePermission(1, TenantId, AppId, RoleId, ResourceId, ActionId);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando não encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByRoleIdAsync

    [Fact(DisplayName = "GetByRoleIdAsync - Deve retornar permissão pela role")]
    [Trait("Infra.Data", "")]
    public async Task GetByRoleIdAsync_Encontrada_DeveRetornar()
    {
        var sut = CreateSutWithData(new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, ActionId));

        var result = await sut.GetByRoleIdAsync(RoleId, default);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "GetByRoleIdAsync - Deve retornar null quando não encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByRoleIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByRoleIdAsync(99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByRoleAsync

    [Fact(DisplayName = "GetByRoleAsync - Deve retornar lista de permissões pela role")]
    [Trait("Infra.Data", "")]
    public async Task GetByRoleAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, ActionId),
            new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, 31));

        var result = await sut.GetByRoleAsync(RoleId, TenantId, default);

        Assert.Equal(2, result.Count);
    }

    #endregion

    #region ExistsAsync

    [Fact(DisplayName = "ExistsAsync - Deve retornar true quando permissão existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, ActionId));

        var result = await sut.ExistsAsync(TenantId, AppId, RoleId, ResourceId, ActionId, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsAsync - Deve retornar false quando permissão não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsAsync(TenantId, AppId, RoleId, ResourceId, ActionId, default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir permissão com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, ActionId);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir permissão com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new RolePermissionEntity(TenantId, AppId, RoleId, ResourceId, ActionId);
        context.RolePermissions.Add(entity);
        await context.SaveChangesAsync();
        var sut = new RolePermissionDataRepository(context);

        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
