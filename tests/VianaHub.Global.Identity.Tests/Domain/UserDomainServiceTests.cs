using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Services;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using FluentValidation.Results;
using Moq;

namespace VianaHub.Global.Identity.Tests.Domain;

public class UserDomainServiceTests
{
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IUserDataRepository> _repoMock = new();
    private readonly Mock<IEntityDomainValidator<UserEntity>> _validatorMock = new();

    private const int TenantId = 1;
    private const int UserId = 10;

    private UserDomainService CreateSut() => new(
        _notifyMock.Object,
        _repoMock.Object,
        _validatorMock.Object);

    private static UserEntity BuildUser(int id = 1, string name = "User Test")
    {
        var entity = new UserEntity(TenantId, name, "login@test.com", "hashedpassword", null, UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    private static ValidationResult ValidResult() => new();

    private static ValidationResult InvalidResult(string error = "Erro de validação") =>
        new(new List<ValidationFailure> { new("Field", error) });

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de usuários")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<UserEntity> { BuildUser(1), BuildUser(2) };
        _repoMock.Setup(x => x.GetAllAsync(TenantId, default)).ReturnsAsync(entities);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(TenantId, default)).ReturnsAsync([]);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(TenantId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar usuário quando encontrado")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarUsuario()
    {
        var entity = BuildUser(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(TenantId, 1, default);

        Assert.NotNull(result);
        Assert.Equal(entity, result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando usuário não encontrado")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((UserEntity)null!);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(TenantId, 99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de usuários")]
    [Trait("Domain", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var listPage = new ListPage<UserEntity> { Items = [BuildUser(1)], TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(TenantId, new PagedFilter("", true, 1, 10, "Name", "asc"), default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByIdAsync

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar true quando usuário existe")]
    [Trait("Domain", "")]
    public async Task ExistsByIdAsync_Existe_DeveRetornarTrue()
    {
        _repoMock.Setup(x => x.ExistsByIdAsync(TenantId, 1, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ExistsByIdAsync(TenantId, 1, default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByIdAsync - Deve retornar false quando usuário não existe")]
    [Trait("Domain", "")]
    public async Task ExistsByIdAsync_NaoExiste_DeveRetornarFalse()
    {
        _repoMock.Setup(x => x.ExistsByIdAsync(TenantId, 99, default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.ExistsByIdAsync(TenantId, 99, default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar usuário com sucesso")]
    [Trait("Domain", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser();
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
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar usuário com sucesso")]
    [Trait("Domain", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForUpdateAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "UpdateAsync - Deve retornar false e notificar quando validação falha")]
    [Trait("Domain", "")]
    public async Task UpdateAsync_ValidacaoFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForUpdateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.UpdateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region ActivateAsync

    [Fact(DisplayName = "ActivateAsync - Deve ativar usuário com sucesso")]
    [Trait("Domain", "")]
    public async Task ActivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForActivateAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "ActivateAsync - Deve retornar false e notificar quando validação falha")]
    [Trait("Domain", "")]
    public async Task ActivateAsync_ValidacaoFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForActivateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.ActivateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region DeactivateAsync

    [Fact(DisplayName = "DeactivateAsync - Deve desativar usuário com sucesso")]
    [Trait("Domain", "")]
    public async Task DeactivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForDeactivateAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false e notificar quando validação falha")]
    [Trait("Domain", "")]
    public async Task DeactivateAsync_ValidacaoFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForDeactivateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve deletar usuário com sucesso")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(entity)).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(entity, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false e notificar quando validação falha")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_ValidacaoFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildUser();
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeleteAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion
}
