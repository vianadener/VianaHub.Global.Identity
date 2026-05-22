using AutoMapper;
using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.User;
using VianaHub.Global.Identity.Application.Dto.Response.User;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Helpers;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.AspNetCore.Http;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class UserAppServiceTests
{
    private readonly Mock<IUserDataRepository> _repoMock = new();
    private readonly Mock<IUserDomainService> _domainMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<IFileValidationService> _fileValidationMock = new();

    private const int TenantId = 1;
    private const int UserId = 10;

    public UserAppServiceTests()
    {
        _currentUserMock.Setup(x => x.GetTenantId()).Returns(TenantId);
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);

        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
    }

    private UserAppService CreateSut() => new(
        _repoMock.Object,
        _domainMock.Object,
        _notifyMock.Object,
        _mapperMock.Object,
        _currentUserMock.Object,
        _localizationMock.Object,
        _fileValidationMock.Object);

    private static UserEntity BuildUser(int id = 1, string name = "User Test", bool active = true)
    {
        var passwordHash = DomainExtensions.HashClientSecret("Senha@123");
        var entity = new UserEntity(TenantId, name, name, passwordHash, null, UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        return entity;
    }

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de usuários mapeados")]
    [Trait("Application", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<UserEntity> { BuildUser(1), BuildUser(2) };
        var mapped = new List<UserResponse> { new() { Id = 1 }, new() { Id = 2 } };
        _repoMock.Setup(x => x.GetAllAsync(TenantId, default)).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<IEnumerable<UserResponse>>(entities)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há usuários")]
    [Trait("Application", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(TenantId, default)).ReturnsAsync([]);
        _mapperMock.Setup(x => x.Map<IEnumerable<UserResponse>>(It.IsAny<IEnumerable<UserEntity>>())).Returns([]);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar usuário quando encontrado")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarUsuario()
    {
        var entity = BuildUser(1);
        var mapped = new UserResponse { Id = 1 };
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<UserResponse>(entity)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.IsType<UserResponse>(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNullENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de usuários")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var entities = new List<UserEntity> { BuildUser(1) };
        var listPage = new ListPage<UserEntity> { Items = entities, TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<UserResponse>(new List<UserResponse> { new() { Id = 1 } }, 1, 10, 1, 1);
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<UserResponse>>(listPage)).Returns(mappedPage);

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
        var listPage = new ListPage<UserEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<UserResponse>([], 1, 10, 0, 0);
        _repoMock.Setup(x => x.GetPagedAsync(TenantId, It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<UserResponse>>(listPage)).Returns(mappedPage);

        var request = new PagedFilterRequest { Search = "", PageNumber = 1, PageSize = 10, SortBy = "Name", SortDirection = "asc" };

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(request, default);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar usuário com sucesso")]
    [Trait("Application", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var request = new CreateUserRequest { Name = "Novo Usuario", Secret = "Senha@123" };
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.True(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false e notificar quando nome já existe")]
    [Trait("Application", "")]
    public async Task CreateAsync_NomeJaExiste_DeveRetornarFalseENotificar()
    {
        var request = new CreateUserRequest { Name = "Usuario Existente", Secret = "Senha@123" };
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false quando domínio falha")]
    [Trait("Application", "")]
    public async Task CreateAsync_DominioFalha_DeveRetornarFalse()
    {
        var request = new CreateUserRequest { Name = "Novo Usuario", Secret = "Senha@123" };
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, request.Name, default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar usuário com sucesso")]
    [Trait("Application", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser(1);
        var request = new UpdateUserRequest { Name = "Usuario Atualizado" };
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(1, request, default);

        Assert.True(result);
        _domainMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "UpdateAsync - Deve retornar false e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task UpdateAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((UserEntity)null);
        var request = new UpdateUserRequest { Name = "Usuario" };

        var sut = CreateSut();
        var result = await sut.UpdateAsync(99, request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region UpdatePasswordAsync

    [Fact(DisplayName = "UpdatePasswordAsync - Deve atualizar senha com sucesso")]
    [Trait("Application", "")]
    public async Task UpdatePasswordAsync_Sucesso_DeveRetornarTrue()
    {
        const string currentSecret = "Senha@123";
        var entity = BuildUser(1);
        var request = new UpdateSecretRequest { CurrentSecret = currentSecret, NewSecret = "NovaSenha@456" };
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.UpdateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdatePasswordAsync(1, request, default);

        Assert.True(result);
        _domainMock.Verify(x => x.UpdateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "UpdatePasswordAsync - Deve retornar false e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task UpdatePasswordAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((UserEntity)null);
        var request = new UpdateSecretRequest { CurrentSecret = "Senha@123", NewSecret = "NovaSenha@456" };

        var sut = CreateSut();
        var result = await sut.UpdatePasswordAsync(99, request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "UpdatePasswordAsync - Deve retornar false e notificar quando senha atual incorreta")]
    [Trait("Application", "")]
    public async Task UpdatePasswordAsync_SenhaAtualIncorreta_DeveRetornarFalseENotificar()
    {
        var entity = BuildUser(1);
        var request = new UpdateSecretRequest { CurrentSecret = "SenhaErrada@999", NewSecret = "NovaSenha@456" };
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.UpdatePasswordAsync(1, request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _domainMock.Verify(x => x.UpdateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region ActivateAsync

    [Fact(DisplayName = "ActivateAsync - Deve ativar usuário com sucesso")]
    [Trait("Application", "")]
    public async Task ActivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser(1, active: false);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.ActivateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.ActivateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "ActivateAsync - Deve retornar false e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task ActivateAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.ActivateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region DeactivateAsync

    [Fact(DisplayName = "DeactivateAsync - Deve desativar usuário com sucesso")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.DeactivateAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.DeactivateAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.DeactivateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir usuário com sucesso")]
    [Trait("Application", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildUser(1);
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 1, default)).ReturnsAsync(entity);
        _domainMock.Setup(x => x.DeleteAsync(entity, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(1, default);

        Assert.True(result);
        _domainMock.Verify(x => x.DeleteAsync(entity, default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false e notificar quando usuário não encontrado")]
    [Trait("Application", "")]
    public async Task DeleteAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(TenantId, 99, default)).ReturnsAsync((UserEntity)null);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _domainMock.Verify(x => x.DeleteAsync(It.IsAny<UserEntity>(), default), Times.Never);
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
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false e notificar quando CSV está vazio")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_CsvVazio_DeveRetornarFalseENotificar()
    {
        var csvContent = "Name;Secret;UrlImage\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando item sem nome")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_ItemSemNome_DeveRetornarFalse()
    {
        var csvContent = "Name;Secret;UrlImage\r\n;Senha@123;\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando item sem secret")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_ItemSemSecret_DeveRetornarFalse()
    {
        var csvContent = "Name;Secret;UrlImage\r\nNovo Usuario;;\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando usuário já existe")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_UsuarioJaExiste_DeveRetornarFalse()
    {
        var csvContent = "Name;Secret;UrlImage\r\nUsuario Existente;Senha@123;\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, It.IsAny<string>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve processar CSV com sucesso")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_Sucesso_DeveRetornarTrue()
    {
        var csvContent = "Name;Secret;UrlImage\r\nNovo Usuario;Senha@123;\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, It.IsAny<string>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.BulkUploadAsync(fileMock.Object, default);

        Assert.True(result);
        _domainMock.Verify(x => x.CreateAsync(It.IsAny<UserEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "BulkUploadAsync - Deve retornar false quando domínio falha ao criar usuário")]
    [Trait("Application", "")]
    public async Task BulkUploadAsync_DominioFalha_DeveRetornarFalse()
    {
        var csvContent = "Name;Secret;UrlImage\r\nNovo Usuario;Senha@123;\r\n";
        var fileMock = BuildFileMock(System.Text.Encoding.UTF8.GetBytes(csvContent));
        _fileValidationMock.Setup(x => x.ValidateFile(fileMock.Object)).Returns(true);
        _repoMock.Setup(x => x.ExistsByNameAsync(TenantId, It.IsAny<string>(), default)).ReturnsAsync(false);
        _domainMock.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), default)).ReturnsAsync(false);

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
        fileMock.Setup(x => x.FileName).Returns("users.csv");
        fileMock.Setup(x => x.ContentType).Returns("text/csv");
        return fileMock;
    }

    #endregion
}
