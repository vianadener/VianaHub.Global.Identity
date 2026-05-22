using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Services;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using FluentValidation.Results;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain;

public class UserRoleDomainServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IUserRoleDataRepository> _repoMock = new();
    private readonly Mock<IEntityDomainValidator<UserRoleEntity>> _validatorMock = new();

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;
    private const int RoleId = 5;

    private UserRoleDomainService CreateSut() => new(
        _notifyMock.Object,
        _repoMock.Object,
        _validatorMock.Object);

    private static UserRoleEntity BuildUserRole(int id = 1) =>
        new(TenantId, AppId, UserId, RoleId);

    private static ValidationResult ValidResult() => new();

    private static ValidationResult InvalidResult(string error = "Erro de validação") =>
        new(new List<ValidationFailure> { new("Field", error) });

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de user roles")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<UserRoleEntity> { BuildUserRole(1), BuildUserRole(2) };
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
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync(new List<UserRoleEntity>());

        var sut = CreateSut();
        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar user role quando encontrada")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarUserRole()
    {
        var entity = BuildUserRole(1);
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
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((UserRoleEntity)null!);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de user roles")]
    [Trait("Domain", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var listPage = new ListPage<UserRoleEntity> { Items = [BuildUserRole(1)], TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, AppId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(TenantId, AppId, new PagedFilter("", true, 1, 10, "Name", "asc"), default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsAsync

    [Fact(DisplayName = "ExistsAsync - Deve retornar true quando relação existe")]
    [Trait("Domain", "")]
    public async Task ExistsAsync_Existe_DeveRetornarTrue()
    {
        _repoMock.Setup(x => x.ExistsAsync(TenantId, AppId, UserId, RoleId, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ExistsAsync(TenantId, AppId, UserId, RoleId, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsAsync - Deve retornar false quando relação não existe")]
    [Trait("Domain", "")]
    public async Task ExistsAsync_NaoExiste_DeveRetornarFalse()
    {
        _repoMock.Setup(x => x.ExistsAsync(TenantId, AppId, UserId, RoleId, default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.ExistsAsync(TenantId, AppId, UserId, RoleId, default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar user role com sucesso")]
    [Trait("Domain", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUserRole();
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.CreateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.CreateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false e notificar quando validação falha")]
    [Trait("Domain", "")]
    public async Task CreateAsync_ValidacaoFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildUserRole();
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve deletar user role com sucesso")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUserRole();
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.DeleteAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.DeleteAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false e notificar quando validação falha")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_ValidacaoFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildUserRole();
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeleteAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.DeleteAsync(It.IsAny<UserRoleEntity>(), default), Times.Never);
    }

    #endregion
}
