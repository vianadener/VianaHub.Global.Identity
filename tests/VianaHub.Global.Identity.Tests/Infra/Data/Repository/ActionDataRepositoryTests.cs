using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class ActionDataRepositoryTests
{
    private const int TenantId = 1;
    private const int AppId = 1;

    private ActionDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private ActionDataRepository CreateSutWithData(params ActionEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.Actions.AddRange(entities);
        context.SaveChanges();
        return new ActionDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "ActionDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "ActionDataRepository - Deve implementar IActionDataRepository")]
    [Trait("Infra.Data", "")]
    public void ActionDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IActionDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de actions do tenant e app")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildAction(1, TenantId, AppId, "Action A"),
            RepositoryTestHelper.BuildAction(2, TenantId, AppId, "Action B"));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há actions")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar actions de outro tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_OutroTenant_NaoDeveRetornar()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildAction(1, 99, AppId, "Action Outro Tenant"));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar actions excluídas")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_ActionExcluida_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildAction(1, TenantId, AppId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar action quando encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrada_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildAction(1, TenantId, AppId));

        var result = await sut.GetByIdAsync(TenantId, AppId, 1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando action não encontrada")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(TenantId, AppId, 99, default);

        Assert.Null(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Não deve retornar action excluída")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Excluida_DeveRetornarNull()
    {
        var entity = RepositoryTestHelper.BuildAction(1, TenantId, AppId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(TenantId, AppId, 1, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de actions")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildAction(1, TenantId, AppId, "Action A"),
            RepositoryTestHelper.BuildAction(2, TenantId, AppId, "Action B"));
        var filter = new PagedFilter("", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, AppId, filter, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count());
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por search")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComSearch_DeveRetornarFiltrado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildAction(1, TenantId, AppId, "Alpha"),
            RepositoryTestHelper.BuildAction(2, TenantId, AppId, "Beta"));
        var filter = new PagedFilter("Alpha", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, AppId, filter, default);

        Assert.Single(result.Items);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por IsActive")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComIsActive_DeveRetornarFiltrado()
    {
        var inativo = RepositoryTestHelper.BuildAction(1, TenantId, AppId, "Inativo");
        inativo.Deactivate(1);
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildAction(2, TenantId, AppId, "Ativo"),
            inativo);
        var filter = new PagedFilter("", true, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, AppId, filter, default);

        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando action existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildAction(1, TenantId, AppId, "Action Test"));

        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Action Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando action não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Inexistente", default);

        Assert.False(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Não deve retornar action excluída")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Excluida_DeveRetornarFalse()
    {
        var entity = RepositoryTestHelper.BuildAction(1, TenantId, AppId, "Action Test");
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Action Test", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir action com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new ActionEntity(TenantId, AppId, "Nova Action", "Desc", 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar action com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new ActionEntity(TenantId, AppId, "Original", "Desc", 1);
        context.Actions.Add(entity);
        await context.SaveChangesAsync();
        var sut = new ActionDataRepository(context);

        entity.Update("Atualizado", "Nova Desc", 1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
