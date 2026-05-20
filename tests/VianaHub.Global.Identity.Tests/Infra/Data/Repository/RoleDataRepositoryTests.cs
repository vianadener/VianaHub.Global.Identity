using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class RoleDataRepositoryTests
{
    private const int TenantId = 1;
    private const int AppId = 1;

    private RoleDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private RoleDataRepository CreateSutWithData(params RoleEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.Roles.AddRange(entities);
        context.SaveChanges();
        return new RoleDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "RoleDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "RoleDataRepository - Deve implementar IRoleDataRepository")]
    [Trait("Infra.Data", "")]
    public void RoleDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IRoleDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de roles do tenant e app")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildRole(1, TenantId, AppId, "Role A"),
            RepositoryTestHelper.BuildRole(2, TenantId, AppId, "Role B"));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há roles")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar roles excluídas")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Excluida_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildRole(1, TenantId, AppId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar role quando encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrada_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildRole(1, TenantId, AppId));

        var result = await sut.GetByIdAsync(TenantId, AppId, 1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando role não encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(TenantId, AppId, 99, default);

        Assert.Null(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Não deve retornar role excluída")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Excluida_DeveRetornarNull()
    {
        var entity = RepositoryTestHelper.BuildRole(1, TenantId, AppId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(TenantId, AppId, 1, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de roles")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildRole(1, TenantId, AppId, "Role A"),
            RepositoryTestHelper.BuildRole(2, TenantId, AppId, "Role B"));
        var filter = new PagedFilter("", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, AppId, filter, default);

        Assert.Equal(2, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por search")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComSearch_DeveRetornarFiltrado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildRole(1, TenantId, AppId, "Alpha"),
            RepositoryTestHelper.BuildRole(2, TenantId, AppId, "Beta"));
        var filter = new PagedFilter("Alpha", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, AppId, filter, default);

        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando role existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildRole(1, TenantId, AppId, "Role Test"));

        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Role Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando role não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Inexistente", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir role com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new RoleEntity(TenantId, AppId, "Nova Role", "Desc", 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar role com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new RoleEntity(TenantId, AppId, "Original", "Desc", 1);
        context.Roles.Add(entity);
        await context.SaveChangesAsync();
        var sut = new RoleDataRepository(context);

        entity.Update("Atualizado", "Nova Desc", 1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
