using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.RolePermission;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class RolePermissionMappingProfileTests
{
    private readonly IMapper _mapper;

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public RolePermissionMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<RolePermissionMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "RolePermissionMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<RolePermissionMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region RolePermissionEntity -> RolePermissionResponse

    [Fact(DisplayName = "RolePermissionEntity -> RolePermissionResponse - Deve mapear navegações preenchidas")]
    [Trait("Application", "")]
    public void RolePermissionEntity_Para_RolePermissionResponse_ComNavegacoes_DeveMapear()
    {
        var entity = BuildRolePermission(1, "Admin", "Relatórios", "Leitura");

        var result = _mapper.Map<RolePermissionResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Admin", result.Role);
        Assert.Equal("Relatórios", result.Resource);
        Assert.Equal("Leitura", result.Action);
    }

    [Fact(DisplayName = "RolePermissionEntity -> RolePermissionResponse - Deve retornar string vazia quando navegações nulas")]
    [Trait("Application", "")]
    public void RolePermissionEntity_Para_RolePermissionResponse_SemNavegacoes_DeveRetornarStringVazia()
    {
        var entity = BuildRolePermission(2, null, null, null);

        var result = _mapper.Map<RolePermissionResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Role);
        Assert.Equal(string.Empty, result.Resource);
        Assert.Equal(string.Empty, result.Action);
    }

    #endregion

    #region RolePermissionEntity -> RolePermissionDetailResponse

    [Fact(DisplayName = "RolePermissionEntity -> RolePermissionDetailResponse - Deve mapear navegações preenchidas")]
    [Trait("Application", "")]
    public void RolePermissionEntity_Para_RolePermissionDetailResponse_ComNavegacoes_DeveMapear()
    {
        var entity = BuildRolePermission(3, "Operador", "Usuários", "Escrita");

        var result = _mapper.Map<RolePermissionDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Operador", result.Role);
        Assert.Equal("Usuários", result.Resource);
        Assert.Equal("Escrita", result.Action);
    }

    [Fact(DisplayName = "RolePermissionEntity -> RolePermissionDetailResponse - Deve retornar string vazia quando navegações nulas")]
    [Trait("Application", "")]
    public void RolePermissionEntity_Para_RolePermissionDetailResponse_SemNavegacoes_DeveRetornarStringVazia()
    {
        var entity = BuildRolePermission(4, null, null, null);

        var result = _mapper.Map<RolePermissionDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.Role);
        Assert.Equal(string.Empty, result.Resource);
        Assert.Equal(string.Empty, result.Action);
    }

    #endregion

    #region ListPage<RolePermissionEntity> -> ListPageResponse<RolePermissionResponse>

    [Fact(DisplayName = "ListPage<RolePermissionEntity> -> ListPageResponse<RolePermissionResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_RolePermissionEntity_Para_ListPageResponse_DeveMapear()
    {
        var items = new List<RolePermissionEntity> { BuildRolePermission(1, "Admin", "Recurso", "Ação") };
        var listPage = new ListPage<RolePermissionEntity> { Items = items, TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<RolePermissionResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<RolePermissionEntity> vazio -> ListPageResponse<RolePermissionResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_RolePermissionEntity_Vazio_Para_ListPageResponse_DeveMapear()
    {
        var listPage = new ListPage<RolePermissionEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<RolePermissionResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static RolePermissionEntity BuildRolePermission(int id, string? roleName, string? resourceName, string? actionName)
    {
        var entity = new RolePermissionEntity(TenantId, AppId, roleId: 1, resourceId: 1, actionId: 1);
        typeof(RolePermissionEntity)
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (roleName != null)
        {
            var role = new RoleEntity(TenantId, AppId, roleName, "Desc", UserId);
            typeof(RolePermissionEntity)
                .GetProperty("Role")!
                .SetValue(entity, role);
        }

        if (resourceName != null)
        {
            var resource = new ResourceEntity(TenantId, AppId, resourceName, "Desc", UserId);
            typeof(RolePermissionEntity)
                .GetProperty("Resource")!
                .SetValue(entity, resource);
        }

        if (actionName != null)
        {
            var action = new ActionEntity(TenantId, AppId, actionName, "Desc", UserId);
            typeof(RolePermissionEntity)
                .GetProperty("Action")!
                .SetValue(entity, action);
        }

        return entity;
    }
}
