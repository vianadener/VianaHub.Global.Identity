using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.UserRole;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class UserRoleMappingProfileTests
{
    private readonly IMapper _mapper;

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public UserRoleMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserRoleMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "UserRoleMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserRoleMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region UserRoleEntity -> UserRoleResponse

    [Fact(DisplayName = "UserRoleEntity -> UserRoleResponse - Deve mapear navegações preenchidas")]
    [Trait("Application", "")]
    public void UserRoleEntity_Para_UserRoleResponse_ComNavegacoes_DeveMapear()
    {
        var entity = BuildUserRole(1, "João Silva", "Admin");

        var result = _mapper.Map<UserRoleResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("João Silva", result.UserName);
        Assert.Equal("Admin", result.RoleName);
    }

    [Fact(DisplayName = "UserRoleEntity -> UserRoleResponse - Deve retornar string vazia quando navegações nulas")]
    [Trait("Application", "")]
    public void UserRoleEntity_Para_UserRoleResponse_SemNavegacoes_DeveRetornarStringVazia()
    {
        var entity = BuildUserRole(2, null, null);

        var result = _mapper.Map<UserRoleResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.UserName);
        Assert.Equal(string.Empty, result.RoleName);
    }

    #endregion

    #region UserRoleEntity -> UserRoleDetailResponse

    [Fact(DisplayName = "UserRoleEntity -> UserRoleDetailResponse - Deve mapear navegações preenchidas")]
    [Trait("Application", "")]
    public void UserRoleEntity_Para_UserRoleDetailResponse_ComNavegacoes_DeveMapear()
    {
        var entity = BuildUserRole(3, "Maria Souza", "Operador");

        var result = _mapper.Map<UserRoleDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Maria Souza", result.UserName);
        Assert.Equal("Operador", result.RoleName);
    }

    [Fact(DisplayName = "UserRoleEntity -> UserRoleDetailResponse - Deve retornar string vazia quando navegações nulas")]
    [Trait("Application", "")]
    public void UserRoleEntity_Para_UserRoleDetailResponse_SemNavegacoes_DeveRetornarStringVazia()
    {
        var entity = BuildUserRole(4, null, null);

        var result = _mapper.Map<UserRoleDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.UserName);
        Assert.Equal(string.Empty, result.RoleName);
    }

    #endregion

    #region ListPage<UserRoleEntity> -> ListPageResponse<UserRoleResponse>

    [Fact(DisplayName = "ListPage<UserRoleEntity> -> ListPageResponse<UserRoleResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_UserRoleEntity_Para_ListPageResponse_DeveMapear()
    {
        var items = new List<UserRoleEntity> { BuildUserRole(1, "User", "Role") };
        var listPage = new ListPage<UserRoleEntity> { Items = items, TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<UserRoleResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<UserRoleEntity> vazio -> ListPageResponse<UserRoleResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_UserRoleEntity_Vazio_Para_ListPageResponse_DeveMapear()
    {
        var listPage = new ListPage<UserRoleEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<UserRoleResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static UserRoleEntity BuildUserRole(int id, string? userName, string? roleName)
    {
        var entity = new UserRoleEntity(TenantId, AppId, userId: 1, roleId: 1);
        typeof(UserRoleEntity)
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (userName != null)
        {
            var user = new UserEntity(TenantId, userName, "login@test.com", "hash", null, UserId);
            typeof(UserRoleEntity)
                .GetProperty("User")!
                .SetValue(entity, user);
        }

        if (roleName != null)
        {
            var role = new RoleEntity(TenantId, AppId, roleName, "Desc", UserId);
            typeof(UserRoleEntity)
                .GetProperty("Role")!
                .SetValue(entity, role);
        }

        return entity;
    }
}
