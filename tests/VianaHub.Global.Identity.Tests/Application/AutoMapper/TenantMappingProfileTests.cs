using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.Tenant;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class TenantMappingProfileTests
{
    private readonly IMapper _mapper;
    private const int UserId = 10;

    public TenantMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<TenantMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "TenantMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<TenantMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region TenantEntity -> TenantResponse

    [Fact(DisplayName = "TenantEntity -> TenantResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void TenantEntity_Para_TenantResponse_DeveMapearCamposCorretamente()
    {
        var entity = BuildTenant(3, "Empresa ABC", "eabc", true);

        var result = _mapper.Map<TenantResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Empresa ABC", result.Name);
        Assert.Equal("eabc", result.Alias);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "TenantEntity -> TenantResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void TenantEntity_Para_TenantResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildTenant(4, "Empresa XYZ", "exyz", false);

        var result = _mapper.Map<TenantResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region TenantEntity -> TenantDetailResponse

    [Fact(DisplayName = "TenantEntity -> TenantDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void TenantEntity_Para_TenantDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = BuildTenant(5, "Empresa DEF", "edef", true, description: "Descrição completa", urlImage: "https://img.com/logo.png");

        var result = _mapper.Map<TenantDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Empresa DEF", result.Name);
        Assert.Equal("edef", result.Alias);
        Assert.Equal("Descrição completa", result.Description);
        Assert.Equal("https://img.com/logo.png", result.UrlImage);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "TenantEntity -> TenantDetailResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void TenantEntity_Para_TenantDetailResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildTenant(6, "Empresa GHI", "eghi", false);

        var result = _mapper.Map<TenantDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ListPage<TenantEntity> -> ListPageResponse<TenantResponse>

    [Fact(DisplayName = "ListPage<TenantEntity> -> ListPageResponse<TenantResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_TenantEntity_Para_ListPageResponse_TenantResponse_DeveMapear()
    {
        var items = new List<TenantEntity> { BuildTenant(1, "T1", "t1", true), BuildTenant(2, "T2", "t2", true) };
        var listPage = new ListPage<TenantEntity> { Items = items, TotalItems = 2, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<TenantResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<TenantEntity> vazio -> ListPageResponse<TenantResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_TenantEntity_Vazio_Para_ListPageResponse_TenantResponse_DeveMapear()
    {
        var listPage = new ListPage<TenantEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<TenantResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static TenantEntity BuildTenant(int id, string name, string alias, bool active, string description = "Descrição", string? urlImage = null)
    {
        var entity = new TenantEntity(name, description, alias, urlImage, null, null, UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!active)
            entity.Deactivate(UserId);
        return entity;
    }
}
