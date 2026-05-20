using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class JobDefinitionDataRepositoryTests
{
    private JobDefinitionDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private JobDefinitionDataRepository CreateSutWithData(params JobDefinitionEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        context.JobDefinitionEntities.AddRange(entities);
        context.SaveChanges();
        return new JobDefinitionDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "JobDefinitionDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "JobDefinitionDataRepository - Deve implementar IJobDefinitionDataRepository")]
    [Trait("Infra.Data", "")]
    public void JobDefinitionDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IJobDefinitionDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de jobs")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJobDefinition(1, "Job A"),
            RepositoryTestHelper.BuildJobDefinition(2, "Job B"));

        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há jobs")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar jobs excluídos")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Excluido_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildJobDefinition(1);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetAllAsync(default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar job quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrado_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJobDefinition(1, "Job Test"));

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

    [Fact(DisplayName = "GetByIdAsync - Não deve retornar job excluído")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Excluido_DeveRetornarNull()
    {
        var entity = RepositoryTestHelper.BuildJobDefinition(1);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetByIdAsync(1, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByNameAsync

    [Fact(DisplayName = "GetByNameAsync - Deve retornar job pelo nome")]
    [Trait("Infra.Data", "")]
    public async Task GetByNameAsync_Encontrado_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJobDefinition(1, "Job Test"));

        var result = await sut.GetByNameAsync("Job Test", default);

        Assert.NotNull(result);
        Assert.Equal("Job Test", result.Name);
    }

    [Fact(DisplayName = "GetByNameAsync - Deve retornar null quando não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByNameAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByNameAsync("Inexistente", default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de jobs")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJobDefinition(1, "Job A"),
            RepositoryTestHelper.BuildJobDefinition(2, "Job B"));
        var filter = new PagedFilter("", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(filter, default);

        Assert.Equal(2, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por search")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComSearch_DeveRetornarFiltrado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJobDefinition(1, "Alpha Job"),
            RepositoryTestHelper.BuildJobDefinition(2, "Beta Job"));
        var filter = new PagedFilter("Alpha", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(filter, default);

        Assert.Single(result.Items);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por IsActive")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComIsActive_DeveRetornarFiltrado()
    {
        var inativo = RepositoryTestHelper.BuildJobDefinition(1, "Inativo");
        inativo.Deactivate(1);
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildJobDefinition(2, "Ativo"),
            inativo);
        var filter = new PagedFilter("", true, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(filter, default);

        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando job existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildJobDefinition(1, "Job Test"));

        var result = await sut.ExistsByNameAsync("Job Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando job não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByNameAsync("Inexistente", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir job com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new JobDefinitionEntity("Category", "Novo Job", "MyType", 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar job com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new JobDefinitionEntity("Category", "Original", "MyType", 1);
        context.JobDefinitionEntities.Add(entity);
        await context.SaveChangesAsync();
        var sut = new JobDefinitionDataRepository(context);

        entity.Update("Atualizado", null, null, null, 5, 5, "default", 3, null, true, 1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve deletar job com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new JobDefinitionEntity("Category", "Para Deletar", "MyType", 1);
        context.JobDefinitionEntities.Add(entity);
        await context.SaveChangesAsync();
        var sut = new JobDefinitionDataRepository(context);

        entity.Delete(1);
        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
