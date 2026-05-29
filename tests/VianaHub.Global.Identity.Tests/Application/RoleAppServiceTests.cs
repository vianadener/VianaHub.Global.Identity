using AutoMapper;
using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Role;
using VianaHub.Global.Identity.Application.Dto.Response.Role;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class RoleAppServiceTests
{
    private readonly Mock<IRoleDataRepository> _repoMock = new();
    private readonly Mock<IRoleDomainService> _domainMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<IFileValidationService> _fileValidationMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public RoleAppServiceTests()
    {
        _currentUserMock.Setup(x => x.GetTenantId()).Returns(TenantId);
        _currentUserMock.Setup(x => x.GetAppId()).Returns(AppId);
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);

        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
    }

    private RoleAppService CreateSut() => new(
        _repoMock.Object,
        _domainMock.Object,
        _notifyMock.Object,
        _mapperMock.Object,
        NullLogger<RoleAppService>.Instance,
        _currentUserMock.Object,
        _localizationMock.Object,
        _fileValidationMock.Object);

    private static RoleEntity BuildRole(int id = 1, string name = "Role Test", bool active = true)
    {
        var entity = new RoleEntity(TenantId, AppId, name, "Descrição", UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de roles mapeadas")]
    [Trait("Application", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<RoleEntity> { BuildRole(1), BuildRole(2) };
        var mapped = new List<RoleResponse> { new(1, AppId, "Role1", true), new(2, AppId, "Role2", true) };
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<IEnumerable<RoleResponse>>(entities)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há roles")]
    [Trait("Application", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync([]);
        _mapperMock.Setup(x => x.Map<IEnumerable<RoleResponse>>(It.IsAny<IEnumerable<RoleEntity>>())).Returns([]);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar role quando encontrada")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarRole()
    {
        var entity = BuildRole(1);
        var mapped = new RoleResponse(1, AppId, "Role1", true);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 1, default)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<RoleResponse>(entity)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.IsType<RoleResponse>(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null e notificar quando role não encontrada")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNullENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 99, default)).ReturnsAsync((RoleEntity)null);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de roles")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var entities = new List<RoleEntity> { BuildRole(1) };
        var listPage = new ListPage<RoleEntity> { Items = entities, TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<RoleResponse>(new List<RoleResponse> { new(1, AppId, "Role1", true) }, 1, 10, 1, 1);
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, AppId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<RoleResponse>>(listPage)).Returns(mappedPage);

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
        var listPage = new ListPage<RoleEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<RoleResponse>([], 1, 10, 0, 0);
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, AppId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<RoleResponse>>(listPage)).Returns(mappedPage);

        var request = new PagedFilterRequest { Search = "", PageNumber = 1, PageSize = 10, SortBy = "Name", SortDirection = "asc" };

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(request, default);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar role com sucesso")]
    [Trait("Application", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var request = new CreateRoleRequest("Nova Role", "Descrição");
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, request.Name, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<RoleEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.True(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false e notificar quando nome já existe")]
    [Trait("Application", "")]
    public async Task CreateAsync_NomeJaExiste_DeveRetornarFalseENotificar()
    {
        var request = new CreateRoleRequest("Role Existente", "Descrição");
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, request.Name, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false quando domínio falha")]
    [Trait("Application", "")]
    public async Task CreateAsync_DominioFalha_DeveRetornarFalse()
    {
        var request = new CreateRoleRequest("Nova Role", "Descrição");
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, request.Name, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<RoleEntity>(), default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar role com sucesso")]
    [Trait("Application", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildRole(1);
        var request = new UpdateRoleRequest("Role Atualizada", "Nova Descrição");
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(1, request, default);

        Assert.True(result);
        _domainMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "UpdateAsync - Deve retornar false e notificar quando role não encontrada")]
    [Trait("Application", "")]
    public async Task UpdateAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 99, default)).ReturnsAsync((RoleEntity)null);
        var request = new UpdateRoleRequest("Role", "Desc");

        var sut = CreateSut();
        var result = await sut.UpdateAsync(99, request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.UpdateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    #endregion

    #region ActivateAsync

    [Fact(DisplayName = "ActivateAsync - Deve ativar role com sucesso")]
    [Trait("Application", "")]
    public async Task ActivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildRole(1, active: false);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.ActivateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.ActivateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "ActivateAsync - Deve retornar false e notificar quando role não encontrada")]
    [Trait("Application", "")]
    public async Task ActivateAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 99, default)).ReturnsAsync((RoleEntity)null);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.ActivateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    #endregion

    #region DeactivateAsync

    [Fact(DisplayName = "DeactivateAsync - Deve desativar role com sucesso")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildRole(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.DeactivateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.DeactivateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false e notificar quando role não encontrada")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 99, default)).ReturnsAsync((RoleEntity)null);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.DeactivateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir role com sucesso")]
    [Trait("Application", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildRole(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.DeleteAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.DeleteAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false e notificar quando role não encontrada")]
    [Trait("Application", "")]
    public async Task DeleteAsync_NaoEncontrada_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, AppId, 99, default)).ReturnsAsync((RoleEntity)null);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.DeleteAsync(It.IsAny<RoleEntity>(), default), Times.Never);
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
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false e notificar quando CSV está vazio")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_CsvVazio_DeveRetornarFalseENotificar()
    {
        var csvContent = "AppId;Name;Description\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando item do CSV não tem nome")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_ItemSemNome_DeveRetornarFalse()
    {
        var csvContent = "AppId;Name;Description\r\n2;;Descrição sem nome\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando item do CSV não tem descrição")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_ItemSemDescricao_DeveRetornarFalse()
    {
        var csvContent = "AppId;Name;Description\r\n2;Role Test;\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false e notificar quando role já existe")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_RoleJaExiste_DeveRetornarFalseENotificar()
    {
        var csvContent = "AppId;Name;Description\r\n2;Role Existente;Descrição\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, "Role Existente", default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve processar CSV com sucesso")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_Sucesso_DeveRetornarTrue()
    {
        var csvContent = "AppId;Name;Description\r\n2;Nova Role;Descrição válida\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, It.IsAny<string>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<RoleEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.True(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<RoleEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando domínio falha ao criar item")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_DominioFalha_DeveRetornarFalse()
    {
        var csvContent = "AppId;Name;Description\r\n2;Nova Role;Descrição válida\r\n";
        var fileBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
        var fileMock = BuildFileMock(fileBytes);

        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, AppId, It.IsAny<string>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<RoleEntity>(), default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
    }

    private static Mock<IFormFile> BuildFileMock(byte[] content)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.OpenReadStream()).Returns(new MemoryStream(content));
        fileMock.Setup(x => x.Length).Returns(content.Length);
        fileMock.Setup(x => x.FileName).Returns("roles.csv");
        fileMock.Setup(x => x.ContentType).Returns("text/csv");
        return fileMock;
    }

    #endregion
}
