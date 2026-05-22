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

public class ResourceDomainServiceTests
{
    private readonly Mock<IResourceDataRepository> _repoMock = new();
    private readonly Mock<IEntityDomainValidator<ResourceEntity>> _validatorMock = new();
    private readonly Mock<INotify> _notifyMock = new();

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    private ResourceDomainService CreateSut() => new(
        _repoMock.Object,
        _validatorMock.Object,
        _notifyMock.Object);

    private static ResourceEntity BuildResource(int id = 1, string name = "Resource Test")
    {
        var entity = new ResourceEntity(TenantId, AppId, name, "Descrição", UserId);
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    private static ValidationResult ValidResult() => new();

    private static ValidationResult InvalidResult(string error = "Erro de validação") =>
        new(new List<ValidationFailure> { new("Field", error) });

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de resources")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<ResourceEntity> { BuildResource(1), BuildResource(2) };
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync(entities);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia")]
    [Trait("Domain", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync([]);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(TenantId, AppId, default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar resource quando encontrado")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarResource()
    {
        var entity = BuildResource(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(TenantId, AppId, 1, default);

        Assert.NotNull(result);
        Assert.Equal(entity, result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null quando resource não encontrado")]
    [Trait("Domain", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 99, default)).ReturnsAsync((ResourceEntity)null!);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(TenantId, AppId, 99, default);

        Assert.Null(result);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de resources")]
    [Trait("Domain", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var listPage = new ListPage<ResourceEntity> { Items = [BuildResource(1)], TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, AppId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(TenantId, AppId, new PagedFilter("", true, 1, 10, "Name", "asc"), default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    #endregion

    #region ExistsByNameAsync

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar true quando resource existe")]
    [Trait("Domain", "")]
    public async Task ExistsByNameAsync_Existe_DeveRetornarTrue()
    {
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, "Resource Test", default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Resource Test", default);

        Assert.True(result);
    }

    [Fact(DisplayName = "ExistsByNameAsync - Deve retornar false quando resource não existe")]
    [Trait("Domain", "")]
    public async Task ExistsByNameAsync_NaoExiste_DeveRetornarFalse()
    {
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, "Inexistente", default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.ExistsByNameAsync(TenantId, AppId, "Inexistente", default);

        Assert.False(result);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar resource com sucesso")]
    [Trait("Domain", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildResource();
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
        var entity = BuildResource();
        _validatorMock.Setup(x => x.ValidateForCreateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.CreateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.CreateAsync(It.IsAny<ResourceEntity>(), default), Times.Never);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar resource com sucesso")]
    [Trait("Domain", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildResource();
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
        var entity = BuildResource();
        _validatorMock.Setup(x => x.ValidateForUpdateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.UpdateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<ResourceEntity>(), default), Times.Never);
    }

    #endregion

    #region ActivateAsync

    [Fact(DisplayName = "ActivateAsync - Deve ativar resource com sucesso")]
    [Trait("Domain", "")]
    public async Task ActivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildResource();
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
        var entity = BuildResource();
        _validatorMock.Setup(x => x.ValidateForActivateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.ActivateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<ResourceEntity>(), default), Times.Never);
    }

    #endregion

    #region DeactivateAsync

    [Fact(DisplayName = "DeactivateAsync - Deve desativar resource com sucesso")]
    [Trait("Domain", "")]
    public async Task DeactivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildResource();
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
        var entity = BuildResource();
        _validatorMock.Setup(x => x.ValidateForDeactivateAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<ResourceEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve deletar resource com sucesso")]
    [Trait("Domain", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildResource();
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
        var entity = BuildResource();
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(entity)).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeleteAsync(entity, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<ResourceEntity>(), default), Times.Never);
    }

    #endregion
}
