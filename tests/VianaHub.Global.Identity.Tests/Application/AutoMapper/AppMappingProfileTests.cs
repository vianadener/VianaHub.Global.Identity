using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.App;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class AppMappingProfileTests
{
    private readonly IMapper _mapper;

    private const int TenantId = 1;
    private const int UserId = 10;

    public AppMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AppMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "AppMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AppMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region AppEntity -> AppResponse

    [Fact(DisplayName = "AppEntity -> AppResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void AppEntity_Para_AppResponse_DeveMapearCamposCorretamente()
    {
        var entity = BuildApp(3, "Minha App", true);

        var result = _mapper.Map<AppResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal(TenantId, result.TenantId);
        Assert.Equal("Minha App", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "AppEntity -> AppResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void AppEntity_Para_AppResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildApp(4, "App Inativa", false);

        var result = _mapper.Map<AppResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region AppEntity -> AppDetailResponse

    [Fact(DisplayName = "AppEntity -> AppDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void AppEntity_Para_AppDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = BuildApp(5, "App Detalhada", true);

        var result = _mapper.Map<AppDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal(TenantId, result.TenantId);
        Assert.Equal("App Detalhada", result.Name);
        Assert.Equal("Descrição da App", result.Description);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "AppEntity -> AppDetailResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void AppEntity_Para_AppDetailResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildApp(6, "App Inativa Detalhe", false);

        var result = _mapper.Map<AppDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ListPage<AppEntity> -> ListPageResponse<AppResponse>

    [Fact(DisplayName = "ListPage<AppEntity> -> ListPageResponse<AppResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_AppEntity_Para_ListPageResponse_AppResponse_DeveMapear()
    {
        var items = new List<AppEntity> { BuildApp(1), BuildApp(2) };
        var listPage = new ListPage<AppEntity> { Items = items, TotalItems = 2, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<AppResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<AppEntity> vazio -> ListPageResponse<AppResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_AppEntity_Vazio_Para_ListPageResponse_AppResponse_DeveMapear()
    {
        var listPage = new ListPage<AppEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<AppResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static AppEntity BuildApp(int id = 1, string name = "App Test", bool active = true)
    {
        var entity = new AppEntity(TenantId, name, "Descrição da App", UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!active)
            entity.Deactivate(UserId);
        return entity;
    }
}
