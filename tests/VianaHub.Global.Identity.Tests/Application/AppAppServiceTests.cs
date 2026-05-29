using AutoMapper;
using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.App;
using VianaHub.Global.Identity.Application.Dto.Response.App;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.AspNetCore.Http;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class AppAppServiceTests
{
    private readonly Mock<IAppDataRepository> _repoMock = new();
    private readonly Mock<IAppDomainService> _domainMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<IFileValidationService> _fileValidationMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private const int TenantId = 1;
    private const int UserId = 10;

    public AppAppServiceTests()
    {
        _currentUserMock.Setup(x => x.GetTenantId()).Returns(TenantId);
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);

        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
    }

    private AppAppService CreateSut() => new(
        _repoMock.Object,
        _domainMock.Object,
        _notifyMock.Object,
        _mapperMock.Object,
        _currentUserMock.Object,
        _localizationMock.Object,
        _fileValidationMock.Object);

    private static AppEntity BuildApp(int id = 1, string name = "App Test", bool active = true)
    {
        var entity = new AppEntity(TenantId, name, "Descrição", UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de apps mapeadas")]
    [Trait("Application", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<AppEntity> { BuildApp(1), BuildApp(2) };
        var mapped = new List<AppResponse> { new(1, TenantId, "App1", true), new(2, TenantId, "App2", true) };
        _repoMock.Setup(x => x.GetAllAsync(TenantId, default)).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<IEnumerable<AppResponse>>(entities)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há apps")]
    [Trait("Application", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(TenantId, default)).ReturnsAsync([]);
        _mapperMock.Setup(x => x.Map<IEnumerable<AppResponse>>(It.IsAny<IEnumerable<AppEntity>>())).Returns([]);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar app quando encontrada")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarApp()
    {
        var entity = BuildApp(1);
        var mapped = new AppResponse(1, TenantId, "App1", true);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<AppResponse>(entity)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.IsType<AppResponse>(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null e notificar quando app não encontrada")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNullENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((AppEntity)null);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de apps")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var entities = new List<AppEntity> { BuildApp(1) };
        var listPage = new ListPage<AppEntity> { Items = entities, TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<AppResponse>(new List<AppResponse> { new(1, TenantId, "App1", true) }, 1, 10, 1, 1);
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<AppResponse>>(listPage)).Returns(mappedPage);

        var request = new PagedFilterRequest { Search = "", PageNumber = 1, PageSize = 10, SortBy = "Name", SortDirection = "asc" };

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(request, default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página vazia quando não há registros")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_ListaVazia_DeveRetornarPaginadoVazio()
    {
        var listPage = new ListPage<AppEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<AppResponse>([], 1, 10, 0, 0);
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<AppResponse>>(listPage)).Returns(mappedPage);

        var request = new PagedFilterRequest { Search = "", PageNumber = 1, PageSize = 10, SortBy = "Name", SortDirection = "asc" };

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(request, default);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar app com sucesso")]
    [Trait("Application", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var request = new CreateAppRequest("Nova App", "Descrição");
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<AppEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.True(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false e notificar quando nome já existe")]
    [Trait("Application", "")]
    public async Task CreateAsync_NomeJaExiste_DeveRetornarFalseENotificar()
    {
        var request = new CreateAppRequest("App Existente", "Descrição");
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false quando domínio falha")]
    [Trait("Application", "")]
    public async Task CreateAsync_DominioFalha_DeveRetornarFalse()
    {
        var request = new CreateAppRequest("Nova App", "Descrição");
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<AppEntity>(), default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar app com sucesso")]
    [Trait("Application", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildApp(1);
        var request = new UpdateAppRequest("App Atualizada", "Nova Descrição");
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(1, request, default);

        Assert.True(result);
        _domainMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "UpdateAsync - Deve retornar false e notificar quando app não encontrada")]
    [Trait("Application", "")]
    public async Task UpdateAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((AppEntity)null);
        var request = new UpdateAppRequest("App", "Desc");

        var sut = CreateSut();
        var result = await sut.UpdateAsync(99, request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.UpdateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    #endregion

    #region ActivateAsync

    [Fact(DisplayName = "ActivateAsync - Deve ativar app com sucesso")]
    [Trait("Application", "")]
    public async Task ActivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildApp(1, active: false);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.ActivateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.ActivateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "ActivateAsync - Deve retornar false e notificar quando app não encontrada")]
    [Trait("Application", "")]
    public async Task ActivateAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((AppEntity)null);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.ActivateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    #endregion

    #region DeactivateAsync

    [Fact(DisplayName = "DeactivateAsync - Deve desativar app com sucesso")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildApp(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.DeactivateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.DeactivateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false e notificar quando app não encontrada")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((AppEntity)null);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.DeactivateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir app com sucesso")]
    [Trait("Application", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildApp(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.DeleteAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.DeleteAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false e notificar quando app não encontrada")]
    [Trait("Application", "")]
    public async Task DeleteAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((AppEntity)null);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.DeleteAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    #endregion

    #region BulkUploadAsync

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando arquivo é inválido")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_ArquivoInvalido_DeveRetornarFalse()
    {
        var fileMock = new Mock<IFormFile>();
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(false);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false e notificar quando CSV está vazio")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_CsvVazio_DeveRetornarFalseENotificar()
    {
        var csvContent = "Name;Description\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando item do CSV não tem nome")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_ItemSemNome_DeveRetornarFalse()
    {
        var csvContent = "Name;Description\r\n;Descrição sem nome\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando item do CSV não tem descrição")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_ItemSemDescricao_DeveRetornarFalse()
    {
        var csvContent = "Name;Description\r\nApp Test;\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false e notificar quando app já existe")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_AppJaExiste_DeveRetornarFalseENotificar()
    {
        var csvContent = "Name;Description\r\nApp Existente;Descrição\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, "App Existente", default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve processar CSV com sucesso")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_Sucesso_DeveRetornarTrue()
    {
        var csvContent = "Name;Description\r\nNova App;Descrição válida\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, It.IsAny<string>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<AppEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.True(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<AppEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando domínio falha ao criar item")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_DominioFalha_DeveRetornarFalse()
    {
        var csvContent = "Name;Description\r\nNova App;Descrição válida\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, It.IsAny<string>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<AppEntity>(), default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
    }

    private static Mock<IFormFile> BuildFileMock(byte[] content)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.OpenReadStream()).Returns(new MemoryStream(content));
        fileMock.Setup(x => x.Length).Returns(content.Length);
        fileMock.Setup(x => x.FileName).Returns("apps.csv");
        fileMock.Setup(x => x.ContentType).Returns("text/csv");
        return fileMock;
    }

    #endregion
}
