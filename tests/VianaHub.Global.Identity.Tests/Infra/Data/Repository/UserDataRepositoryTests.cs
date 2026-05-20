using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using VianaHub.Global.Identity.Infra.Data.Repository;

namespace VianaHub.Global.Identity.Tests.Infra.Data.Repository;

public class UserDataRepositoryTests
{
    private const int TenantId = 1;

    private UserDataRepository CreateSut() =>
        new(RepositoryTestHelper.CreateContext());

    private UserDataRepository CreateSutWithData(params UserEntity[] entities)
    {
        var context = RepositoryTestHelper.CreateContext();
        var tenant = RepositoryTestHelper.BuildTenant(TenantId);
        context.Tenants.Add(tenant);
        context.Users.AddRange(entities);
        context.SaveChanges();
        return new UserDataRepository(context);
    }

    #region Construtor

    [Fact(DisplayName = "UserDataRepository - Deve ser instanciado com sucesso")]
    [Trait("Infra.Data", "")]
    public void Constructor_Sucesso_DeveInstanciar()
    {
        var sut = CreateSut();

        Assert.NotNull(sut);
    }

    [Fact(DisplayName = "UserDataRepository - Deve implementar IUserDataRepository")]
    [Trait("Infra.Data", "")]
    public void UserDataRepository_DeveImplementarInterface()
    {
        var sut = CreateSut();

        Assert.IsAssignableFrom<IUserDataRepository>(sut);
    }

    #endregion

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de usuários do tenant")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildUser(1, TenantId, "user1@test.com"),
            RepositoryTestHelper.BuildUser(2, TenantId, "user2@test.com"));

        var result = await sut.GetAllAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há usuários")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_SemDados_DeveRetornarVazio()
    {
        var sut = CreateSut();

        var result = await sut.GetAllAsync(TenantId, default);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "GetAllAsync - Não deve retornar usuários excluídos")]
    [Trait("Infra.Data", "")]
    public async Task GetAllAsync_Excluido_NaoDeveRetornar()
    {
        var entity = RepositoryTestHelper.BuildUser(1, TenantId);
        entity.Delete(1);
        var sut = CreateSutWithData(entity);

        var result = await sut.GetAllAsync(TenantId, default);

        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar usuário quando encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_Encontrado_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildUser(1, TenantId));

        var result = await sut.GetByIdAsync(TenantId, 1, default);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando usuário não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(TenantId, 99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetByNormalizedLoginAsync

    [Fact(DisplayName = "GetByNormalizedLoginAsync - Deve retornar usuário pelo login normalizado")]
    [Trait("Infra.Data", "")]
    public async Task GetByNormalizedLoginAsync_Encontrado_DeveRetornar()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildUser(1, TenantId, "user@test.com"));

        var result = await sut.GetByNormalizedLoginAsync(TenantId, "USER@TEST.COM", default);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "GetByNormalizedLoginAsync - Deve retornar null quando login não encontrado")]
    [Trait("Infra.Data", "")]
    public async Task GetByNormalizedLoginAsync_NaoEncontrado_DeveRetornarNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByNormalizedLoginAsync(TenantId, "INEXISTENTE@TEST.COM", default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de usuários")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var sut = CreateSutWithData(
            RepositoryTestHelper.BuildUser(1, TenantId, "user1@test.com"),
            RepositoryTestHelper.BuildUser(2, TenantId, "user2@test.com"));
        var filter = new PagedFilter("", null, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, filter, default);

        Assert.Equal(2, result.TotalItems);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve filtrar por IsActive")]
    [Trait("Infra.Data", "")]
    public async Task GetPagedAsync_ComIsActive_DeveRetornarFiltrado()
    {
        var context = RepositoryTestHelper.CreateContext();
        context.Tenants.Add(RepositoryTestHelper.BuildTenant(TenantId));
        var inativo = RepositoryTestHelper.BuildUser(1, TenantId, "inativo@test.com");
        inativo.Deactivate(1);
        var ativo = RepositoryTestHelper.BuildUser(2, TenantId, "ativo@test.com");
        context.Users.AddRange(ativo, inativo);
        context.SaveChanges();
        var sut = new UserDataRepository(context);
        var filter = new PagedFilter("", true, 1, 10, "Name", "asc");

        var result = await sut.GetPagedAsync(TenantId, filter, default);

        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByIdAsync

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar true quando usuário existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByIdAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildUser(1, TenantId));

        var result = await sut.ExistsByIdAsync(TenantId, 1, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar false quando usuário não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByIdAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByIdAsync(TenantId, 99, default);

        Assert.False(result);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando usuário existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        var sut = CreateSutWithData(RepositoryTestHelper.BuildUser(1, TenantId));

        var result = await sut.ExistsByNameAsync(TenantId, "User Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando usuário não existe")]
    [Trait("Infra.Data", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        var sut = CreateSut();

        var result = await sut.ExistsByNameAsync(TenantId, "Inexistente", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve persistir usuário com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var sut = CreateSut();
        var entity = new UserEntity(TenantId, "Novo User", "novo@test.com", "hash", null, 1);

        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar usuário com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new UserEntity(TenantId, "Original", "orig@test.com", "hash", null, 1);
        context.Users.Add(entity);
        await context.SaveChangesAsync();
        var sut = new UserDataRepository(context);

        entity.Update("Atualizado", null, 1);
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir usuário com sucesso")]
    [Trait("Infra.Data", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var context = RepositoryTestHelper.CreateContext();
        var entity = new UserEntity(TenantId, "User", "del@test.com", "hash", null, 1);
        context.Users.Add(entity);
        await context.SaveChangesAsync();
        var sut = new UserDataRepository(context);

        entity.Delete(1);
        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
    }

    #endregion
}
