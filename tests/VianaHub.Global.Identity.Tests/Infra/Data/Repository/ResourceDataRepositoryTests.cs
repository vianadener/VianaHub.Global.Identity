using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class ResourceDataRepositoryTests
{
    private const int TenantId = 1;
    private const int AppId = 1;

    private ResourceDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private ResourceDataRepository CreateSutWithData(params ResourceEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.Resources.AddRange(entities);
        context.SaveChanges();
        return new ResourceDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "ResourceDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "ResourceDataRepository - Deve implementar IResourceDataRepository")]
    [Trait("Infra.Data", "")]
    public void ResourceDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IResourceDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de resources do tenant e app")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildResource(1, TenantId, AppId, "Res A"),
            RepositoryTestHelper.BuildResource(2, TenantId, AppId, "Res B"));

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há resources")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar resources excluídos")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Excluido_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildResource(1, TenantId, AppId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar resource quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrado_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildResource(1, TenantId, AppId));

        var result = await sut.GetByIdAsync(TenantId, AppId, 1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando resource não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(TenantId, AppId, 99, default);

        Assert.Null(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Não deve retornar resource excluído")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Excluido_DeveRetornarNull()
    {
        var entity = RepositoryTestHelper.BuildResource(1, TenantId, AppId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(TenantId, AppId, 1, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de resources")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildResource(1, TenantId, AppId, "Res A"),
            RepositoryTestHelper.BuildResource(2, TenantId, AppId, "Res B"));
        var filter = new PagedFilter("", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, AppId, filter, default);

        Assert.Equal(2, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por search")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComSearch_DeveRetornarFiltrado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildResource(1, TenantId, AppId, "Alpha"),
            RepositoryTestHelper.BuildResource(2, TenantId, AppId, "Beta"));
        var filter = new PagedFilter("Alpha", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, AppId, filter, default);

        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando resource existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildResource(1, TenantId, AppId, "Resource Test"));

        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Resource Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando resource não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Inexistente", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir resource com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new ResourceEntity(TenantId, AppId, "Novo Resource", "Desc", 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar resource com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new ResourceEntity(TenantId, AppId, "Original", "Desc", 1);
        context.Resources.Add(entity);
        await context.SaveChangesAsync();
        var sut = new ResourceDataRepository(context);

        entity.Update("Atualizado", "Nova Desc", 1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
