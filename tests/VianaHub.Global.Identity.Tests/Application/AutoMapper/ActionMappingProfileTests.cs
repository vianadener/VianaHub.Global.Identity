using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.Action;
using VianaHub.Global.Identity.Application.Dto.Response.Jwt;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class ActionMappingProfileTests
{
    private readonly IMapper _mapper;

    private const int TenantId = 1;
    private const int AppId = 2;
    private const int UserId = 10;

    public ActionMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ActionMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "ActionMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ActionMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region ActionEntity -> ActionResponse

    [Fact(DisplayName = "ActionEntity -> ActionResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void ActionEntity_Para_ActionResponse_DeveMapearCamposCorretamente()
    {
        var entity = BuildAction(5, "Minha Action", true);

        var result = _mapper.Map<ActionResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("Minha Action", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "ActionEntity -> ActionResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void ActionEntity_Para_ActionResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildAction(3, "Action Inativa", false);

        var result = _mapper.Map<ActionResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Action Inativa", result.Name);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ActionEntity -> ActionDetailResponse

    [Fact(DisplayName = "ActionEntity -> ActionDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void ActionEntity_Para_ActionDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = BuildAction(7, "Action Detalhada", true);

        var result = _mapper.Map<ActionDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(7, result.Id);
        Assert.Equal("Action Detalhada", result.Name);
        Assert.Equal("Descrição", result.Description);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "ActionEntity -> ActionDetailResponse - Deve mapear entidade inativa")]
    [Trait("Application", "")]
    public void ActionEntity_Para_ActionDetailResponse_DeveMapearEntidadeInativa()
    {
        var entity = BuildAction(8, "Action Inativa Detalhe", false);

        var result = _mapper.Map<ActionDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(8, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ListPage<ActionEntity> -> ListPageResponse<ActionResponse>

    [Fact(DisplayName = "ListPage<ActionEntity> -> ListPageResponse<ActionResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_ActionEntity_Para_ListPageResponse_ActionResponse_DeveMapear()
    {
        var items = new List<ActionEntity> { BuildAction(1), BuildAction(2) };
        var listPage = new ListPage<ActionEntity> { Items = items, TotalItems = 2, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<ActionResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<ActionEntity> vazio -> ListPageResponse<ActionResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_ActionEntity_Vazio_Para_ListPageResponse_ActionResponse_DeveMapear()
    {
        var listPage = new ListPage<ActionEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<ActionResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    #region JwtKeyEntity -> JwtKeyResponse

    [Fact(DisplayName = "JwtKeyEntity -> JwtKeyResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void JwtKeyEntity_Para_JwtKeyResponse_DeveMapearCamposCorretamente()
    {
        var entity = new JwtKeyEntity(TenantId, "publicKey", "privateKeyEncrypted", UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, 1);

        var result = _mapper.Map<JwtKeyResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(TenantId, result.TenantId);
        Assert.Equal("publicKey", result.PublicKey);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "JwtKeyEntity -> JwtKeyDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void JwtKeyEntity_Para_JwtKeyDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = new JwtKeyEntity(TenantId, "publicKey", "privateKeyEncrypted", UserId,
            algorithm: "RS256", keySize: 2048, keyType: "RSA",
            rotationPolicyDays: 90, overlapPeriodDays: 7, maxTokenLifetimeMinutes: 60);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, 2);

        var result = _mapper.Map<JwtKeyDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal(TenantId, result.TenantId);
        Assert.Equal("RS256", result.Algorithm);
        Assert.Equal(2048, result.KeySize);
        Assert.Equal("RSA", result.KeyType);
        Assert.Equal(90, result.RotationPolicyDays);
        Assert.Equal(7, result.OverlapPeriodDays);
        Assert.Equal(60, result.MaxTokenLifetimeMinutes);
        Assert.True(result.IsActive);
    }

    #endregion

    private static ActionEntity BuildAction(int id = 1, string name = "Action Test", bool active = true)
    {
        var entity = new ActionEntity(TenantId, AppId, name, "Descrição", UserId);
        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);
        if (!active)
            entity.Deactivate(UserId);
        return entity;
    }
}
