using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class AppDataRepositoryTests
{
    private const int TenantId = 1;

    private AppDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private AppDataRepository CreateSutWithData(params AppEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.Apps.AddRange(entities);
        context.SaveChanges();
        return new AppDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "AppDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "AppDataRepository - Deve implementar IAppDataRepository")]
    [Trait("Infra.Data", "")]
    public void AppDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IAppDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de apps do tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildApp(1, TenantId),
            RepositoryTestHelper.BuildApp(2, TenantId));

        var result = await sut.GetAllAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há apps")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar apps de outro tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_OutroTenant_NaoDeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildApp(1, 99));

        var result = await sut.GetAllAsync(TenantId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar apps excluídos")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_AppExcluido_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildApp(1, TenantId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetAllAsync(TenantId, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar app quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrado_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildApp(1, TenantId));

        var result = await sut.GetByIdAsync(TenantId, 1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando app não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(TenantId, 99, default);

        Assert.Null(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Não deve retornar app excluído")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Excluido_DeveRetornarNull()
    {
        var entity = RepositoryTestHelper.BuildApp(1, TenantId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(TenantId, 1, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de apps")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildApp(1, TenantId),
            RepositoryTestHelper.BuildApp(2, TenantId));
        var filter = new PagedFilter("", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, filter, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por IsActive")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComIsActive_DeveRetornarFiltrado()
    {
        var inativo = RepositoryTestHelper.BuildApp(1, TenantId);
        inativo.Deactivate(1);
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildApp(2, TenantId),
            inativo);
        var filter = new PagedFilter("", true, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, filter, default);

        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByIdAsync

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar true quando app existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByIdAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildApp(1, TenantId));

        var result = await sut.ExistsByIdAsync(TenantId, 1, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar false quando app não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByIdAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByIdAsync(TenantId, 99, default);

        Assert.False(result);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando app existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildApp(1, TenantId));

        var result = await sut.ExistsByNameAsync(TenantId, "App Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando app não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByNameAsync(TenantId, "Inexistente", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir app com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new AppEntity(TenantId, "Novo App", "Desc", 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar app com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new AppEntity(TenantId, "Original", "Desc", 1);
        context.Apps.Add(entity);
        await context.SaveChangesAsync();
        var sut = new AppDataRepository(context);

        entity.Update("Atualizado", "Nova Desc", 1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
