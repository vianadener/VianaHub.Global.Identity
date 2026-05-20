using AutoMapper;
using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Request.UserRole;
using VianaHub.Global.Identity.Application.Dto.Response.UserRole;
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

public class UserRoleAppServiceTests
{
    private readonly Mock<IUserRoleDomainService> _domainMock = new();
    private readonly Mock<IUserRoleDataRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFileValidationService> _fileValidationMock = new();

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public UserRoleAppServiceTests()
    {
        _currentUserMock.Setup(x => x.GetTenantId()).Returns(TenantId);
        _currentUserMock.Setup(x => x.GetAppId()).Returns(AppId);
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);

        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
    }

    private UserRoleAppService CreateSut() => new(
        _domainMock.Object,
        _repoMock.Object,
        _mapperMock.Object,
        _notifyMock.Object,
        _localizationMock.Object,
        _currentUserMock.Object,
        _fileValidationMock.Object);

    private static UserRoleEntity BuildEntity(int id = 1)
    {
        var entity = new UserRoleEntity(TenantId, AppId, UserId, 1);
        typeof(UserRoleEntity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de user roles mapeadas")]
    [Trait("Application", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<UserRoleEntity> { BuildEntity(1), BuildEntity(2) };
        var mapped = new List<UserRoleResponse> { new() { Id = 1 }, new() { Id = 2 } };
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<IList<UserRoleResponse>>(entities)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há user roles")]
    [Trait("Application", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(TenantId, AppId, default)).ReturnsAsync([]);
        _mapperMock.Setup(x => x.Map<IList<UserRoleResponse>>(It.IsAny<IList<UserRoleEntity>>())).Returns([]);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar user role quando encontrada")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarUserRole()
    {
        var entity = BuildEntity(1);
        var mapped = new UserRoleResponse { Id = 1 };
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<UserRoleResponse>(entity)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.IsType<UserRoleResponse>(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null e notificar quando user role não encontrada")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_NaoEncontrada_DeveRetornarNullENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((UserRoleEntity)null);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de user roles")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var entities = new List<UserRoleEntity> { BuildEntity(1) };
        var listPage = new ListPage<UserRoleEntity> { Items = entities, TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        var filter = new PagedFilter("", null, 1, 10, "Id", "asc");
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, AppId, filter, default)).ReturnsAsync(listPage);

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(filter, default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página vazia quando não há registros")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_ListaVazia_DeveRetornarPaginadoVazio()
    {
        var listPage = new ListPage<UserRoleEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };
        var filter = new PagedFilter("", null, 1, 10, "Id", "asc");
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, AppId, filter, default)).ReturnsAsync(listPage);

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(filter, default);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar user role com sucesso")]
    [Trait("Application", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarResponse()
    {
        var request = new CreateUserRoleRequest { AppId = AppId, UserId = UserId, RoleId = 1 };
        var entity = BuildEntity(1);
        var mapped = new UserRoleResponse { Id = 1 };
        _repoMock.Setup(x => x.ExistsAsync(TenantId, request.AppId, request.UserId, request.RoleId, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default)).ReturnsAsync(true);
        _mapperMock.Setup(x => x.Map<UserRoleResponse>(It.IsAny<object>())).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.NotNull(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar null e notificar quando user role já existe")]
    [Trait("Application", "")]
    public async Task CreateAsync_UserRoleJaExiste_DeveRetornarNullENotificar()
    {
        var request = new CreateUserRoleRequest { AppId = AppId, UserId = UserId, RoleId = 1 };
        _repoMock.Setup(x => x.ExistsAsync(TenantId, request.AppId, request.UserId, request.RoleId, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 404), Times.Once);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir user role com sucesso")]
    [Trait("Application", "")]
    public async Task DeleteAsync_Sucesso_DeveExcluir()
    {
        var entity = BuildEntity(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _repoMock.Setup(x => x.DeleteAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        await sut.DeleteAsync(1, default);

        _repoMock.Verify(x => x.DeleteAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve notificar quando user role não encontrada")]
    [Trait("Application", "")]
    public async Task DeleteAsync_NaoEncontrada_DeveNotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((UserRoleEntity)null);

        var sut = CreateSut();
        await sut.DeleteAsync(99, default);

        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _repoMock.Verify(x => x.DeleteAsync(It.IsAny<UserRoleEntity>(), default), Times.Never);
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
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false e notificar quando CSV está vazio")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_CsvVazio_DeveRetornarFalseENotificar()
    {
        var csvContent = "UserId;RoleId\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando item com UserId inválido")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_UserIdInvalido_DeveRetornarFalse()
    {
        var csvContent = "UserId;RoleId\r\n0;1\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando user role já existe")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_UserRoleJaExiste_DeveRetornarFalse()
    {
        var csvContent = "UserId;RoleId\r\n10;1\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsAsync(TenantId, AppId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve processar CSV com sucesso")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_Sucesso_DeveRetornarTrue()
    {
        var csvContent = "UserId;RoleId\r\n10;1\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsAsync(TenantId, AppId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.True(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando domínio falha ao criar item")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_DominioFalha_DeveRetornarFalse()
    {
        var csvContent = "UserId;RoleId\r\n10;1\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsAsync(TenantId, AppId, It.IsAny<int>(), It.IsAny<int>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<UserRoleEntity>(), default)).ReturnsAsync(false);

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
        fileMock.Setup(x => x.FileName).Returns("userroles.csv");
        fileMock.Setup(x => x.ContentType).Returns("text/csv");
        return fileMock;
    }

    #endregion
}
