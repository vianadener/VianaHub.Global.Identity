using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.Resource;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class ResourceMappingProfileTests
{
    private readonly IMapper _mapper;

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public ResourceMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ResourceMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "ResourceMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ResourceMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region ResourceEntity -> ResourceResponse

    [Fact(DisplayName = "ResourceEntity -> ResourceResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void ResourceEntity_Para_ResourceResponse_DeveMapearCamposCorretamente()
    {
        var entity = BuildResource(3, "Relatórios", true);

        var result = _mapper.Map<ResourceResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Relatórios", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "ResourceEntity -> ResourceResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void ResourceEntity_Para_ResourceResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildResource(4, "Resource Inativo", false);

        var result = _mapper.Map<ResourceResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ResourceEntity -> ResourceDetailResponse

    [Fact(DisplayName = "ResourceEntity -> ResourceDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void ResourceEntity_Para_ResourceDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = BuildResource(5, "Painel", true);

        var result = _mapper.Map<ResourceDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Painel", result.Name);
        Assert.Equal("Descrição do Resource", result.Description);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "ResourceEntity -> ResourceDetailResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void ResourceEntity_Para_ResourceDetailResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildResource(6, "Resource Inativo Detalhe", false);

        var result = _mapper.Map<ResourceDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ListPage<ResourceEntity> -> ListPageResponse<ResourceResponse>

    [Fact(DisplayName = "ListPage<ResourceEntity> -> ListPageResponse<ResourceResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_ResourceEntity_Para_ListPageResponse_ResourceResponse_DeveMapear()
    {
        var items = new List<ResourceEntity> { BuildResource(1), BuildResource(2) };
        var listPage = new ListPage<ResourceEntity> { Items = items, TotalItems = 2, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<ResourceResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<ResourceEntity> vazio -> ListPageResponse<ResourceResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_ResourceEntity_Vazio_Para_ListPageResponse_ResourceResponse_DeveMapear()
    {
        var listPage = new ListPage<ResourceEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<ResourceResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static ResourceEntity BuildResource(int id = 1, string name = "Resource Test", bool active = true)
    {
        var entity = new ResourceEntity(TenantId, AppId, name, "Descrição do Resource", UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!active)
            entity.Deactivate(UserId);
        return entity;
    }
}
