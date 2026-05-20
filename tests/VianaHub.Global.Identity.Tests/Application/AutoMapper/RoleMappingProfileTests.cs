using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.Role;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class RoleMappingProfileTests
{
    private readonly IMapper _mapper;

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public RoleMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<RoleMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "RoleMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<RoleMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region RoleEntity -> RoleResponse

    [Fact(DisplayName = "RoleEntity -> RoleResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void RoleEntity_Para_RoleResponse_DeveMapearCamposCorretamente()
    {
        var entity = BuildRole(3, "Admin", true);

        var result = _mapper.Map<RoleResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Admin", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "RoleEntity -> RoleResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void RoleEntity_Para_RoleResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildRole(4, "Role Inativa", false);

        var result = _mapper.Map<RoleResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region RoleEntity -> RoleDetailResponse

    [Fact(DisplayName = "RoleEntity -> RoleDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void RoleEntity_Para_RoleDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = BuildRole(5, "Gerente", true);

        var result = _mapper.Map<RoleDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Gerente", result.Name);
        Assert.Equal("Descrição da Role", result.Description);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "RoleEntity -> RoleDetailResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void RoleEntity_Para_RoleDetailResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildRole(6, "Role Inativa Detalhe", false);

        var result = _mapper.Map<RoleDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ListPage<RoleEntity> -> ListPageResponse<RoleResponse>

    [Fact(DisplayName = "ListPage<RoleEntity> -> ListPageResponse<RoleResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_RoleEntity_Para_ListPageResponse_RoleResponse_DeveMapear()
    {
        var items = new List<RoleEntity> { BuildRole(1), BuildRole(2) };
        var listPage = new ListPage<RoleEntity> { Items = items, TotalItems = 2, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<RoleResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<RoleEntity> vazio -> ListPageResponse<RoleResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_RoleEntity_Vazio_Para_ListPageResponse_RoleResponse_DeveMapear()
    {
        var listPage = new ListPage<RoleEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<RoleResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static RoleEntity BuildRole(int id = 1, string name = "Role Test", bool active = true)
    {
        var entity = new RoleEntity(TenantId, AppId, name, "Descrição da Role", UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!active)
            entity.Deactivate(UserId);
        return entity;
    }
}
