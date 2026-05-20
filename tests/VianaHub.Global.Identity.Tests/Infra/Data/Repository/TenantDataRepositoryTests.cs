using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class TenantDataRepositoryTests
{
    private TenantDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private TenantDataRepository CreateSutWithData(params TenantEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.Tenants.AddRange(entities);
        context.SaveChanges();
        return new TenantDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "TenantDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "TenantDataRepository - Deve implementar ITenantDataRepository")]
    [Trait("Infra.Data", "")]
    public void TenantDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<ITenantDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de tenants")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildTenant(1),
            RepositoryTestHelper.BuildTenant(2));

        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há tenants")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar tenants excluídos")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Excluido_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildTenant(1);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetAllAsync(default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar tenant quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrado_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildTenant(1));

        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando tenant não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Não deve retornar tenant excluído")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Excluido_DeveRetornarNull()
    {
        var entity = RepositoryTestHelper.BuildTenant(1);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(1, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de tenants")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildTenant(1),
            RepositoryTestHelper.BuildTenant(2));
        var filter = new PagedFilter("", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(filter, default);

        Assert.Equal(2, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por IsActive")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComIsActive_DeveRetornarFiltrado()
    {
        var inativo = RepositoryTestHelper.BuildTenant(1);
        inativo.Deactivate(1);
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildTenant(2),
            inativo);
        var filter = new PagedFilter("", true, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(filter, default);

        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByIdAsync

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar true quando tenant existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByIdAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildTenant(1));

        var result = await sut.ExistsByIdAsync(1, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar false quando tenant não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByIdAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByIdAsync(99, default);

        Assert.False(result);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando tenant existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildTenant(1));

        var result = await sut.ExistsByNameAsync("Tenant Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando tenant não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByNameAsync("Inexistente", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir tenant com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new TenantEntity("Novo Tenant", "Desc", "alias", null, null, null, 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar tenant com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new TenantEntity("Original", "Desc", "alias", null, null, null, 1);
        context.Tenants.Add(entity);
        await context.SaveChangesAsync();
        var sut = new TenantDataRepository(context);

        entity.Update("Atualizado", "Nova Desc", "alias2", null, null, null, 1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
