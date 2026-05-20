using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.User;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class UserMappingProfileTests
{
    private readonly IMapper _mapper;

    private const int TenantId = 1;
    private const int UserId = 10;

    public UserMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "UserMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region UserEntity -> UserResponse

    [Fact(DisplayName = "UserEntity -> UserResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void UserEntity_Para_UserResponse_DeveMapearCamposCorretamente()
    {
        var entity = BuildUser(3, "Carlos Oliveira", true);

        var result = _mapper.Map<UserResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Carlos Oliveira", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "UserEntity -> UserResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void UserEntity_Para_UserResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildUser(4, "User Inativo", false);

        var result = _mapper.Map<UserResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region UserEntity -> UserDetailResponse

    [Fact(DisplayName = "UserEntity -> UserDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void UserEntity_Para_UserDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = BuildUser(5, "Ana Lima", true, urlImage: "https://img.example.com/ana.jpg");

        var result = _mapper.Map<UserDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal(TenantId, result.TenantId);
        Assert.Equal("Ana Lima", result.Name);
        Assert.Equal("https://img.example.com/ana.jpg", result.UrlImage);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "UserEntity -> UserDetailResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void UserEntity_Para_UserDetailResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildUser(6, "User Inativo Detalhe", false);

        var result = _mapper.Map<UserDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ListPage<UserEntity> -> ListPageResponse<UserResponse>

    [Fact(DisplayName = "ListPage<UserEntity> -> ListPageResponse<UserResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_UserEntity_Para_ListPageResponse_UserResponse_DeveMapear()
    {
        var items = new List<UserEntity> { BuildUser(1), BuildUser(2) };
        var listPage = new ListPage<UserEntity> { Items = items, TotalItems = 2, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<UserResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<UserEntity> vazio -> ListPageResponse<UserResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_UserEntity_Vazio_Para_ListPageResponse_UserResponse_DeveMapear()
    {
        var listPage = new ListPage<UserEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<UserResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static UserEntity BuildUser(int id = 1, string name = "User Test", bool active = true, string? urlImage = null)
    {
        var entity = new UserEntity(TenantId, name, "user@test.com", "hash", urlImage, UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!active)
            entity.Deactivate(UserId);
        return entity;
    }
}
