using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Services;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain;

public class RolePermissionDomainServiceTests
{
    private readonly Mock<IRolePermissionDataRepository> _repoMock = new();
    private readonly Mock<IValidator<RolePermissionEntity>> _validatorMock = new();

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int RoleId = 3;
    private const int ResourceId = 4;
    private const int ActionId = 5;

    private RolePermissionDomainService CreateSut() => new(
        _repoMock.Object,
        _validatorMock.Object);

    private static RolePermissionEntity BuildRolePermission(int id = 1) =>
        new(TenantId, AppId, RoleId, ResourceId, ActionId);

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de role permissions")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<RolePermissionEntity> { BuildRolePermission(1), BuildRolePermission(2) };
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync(entities);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync(new List<RolePermissionEntity>());

        var sut = CreateSut();
        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar role permission quando encontrada")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarRolePermission()
    {
        var entity = BuildRolePermission(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.Equal(entity, result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando não encontrada")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((RolePermissionEntity)null!);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de role permissions")]
    [Trait("Domain", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var listPage = new ListPage<RolePermissionEntity> { Items = [BuildRolePermission(1)], TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, AppId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(TenantId, AppId, new PagedFilter("", true, 1, 10, "Name", "asc"), default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsAsync

    [Fact(DisplayName = "ExistsAsync - Deve retornar true quando permissão existe")]
    [Trait("Domain", "")]
    public async Task ExistsAsync_Existe_DeveRetornarTrue()
    {
        _repoMock.Setup(x => x.ExistsAsync(TenantId, AppId, RoleId, ResourceId, ActionId, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ExistsAsync(TenantId, AppId, RoleId, ResourceId, ActionId, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsAsync - Deve retornar false quando permissão não existe")]
    [Trait("Domain", "")]
    public async Task ExistsAsync_NaoExiste_DeveRetornarFalse()
    {
        _repoMock.Setup(x => x.ExistsAsync(TenantId, AppId, RoleId, ResourceId, ActionId, default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.ExistsAsync(TenantId, AppId, RoleId, ResourceId, ActionId, default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar role permission com sucesso")]
    [Trait("Domain", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildRolePermission();
        _repoMock.Setup(x => x.CreateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.CreateAsync(entity, default), Times.Once);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve deletar role permission com sucesso")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildRolePermission();
        _repoMock.Setup(x => x.DeleteAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.DeleteAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false quando repositório falha")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_FalhaNoRepositorio_DeveRetornarFalse()
    {
        var entity = BuildRolePermission();
        _repoMock.Setup(x => x.DeleteAsync(entity, default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(entity, default);

        Assert.False(result);
    }

    #endregion
}
