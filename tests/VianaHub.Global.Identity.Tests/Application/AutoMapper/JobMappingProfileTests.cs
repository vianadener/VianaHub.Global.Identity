using AutoMapper;
using VianaHub.Global.Identity.Application.AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Response.Job;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using Microsoft.Extensions.Logging.Abstractions;

namespace VianaHub.Global.Identity.Tests.Application.AutoMapper;

public class JobMappingProfileTests
{
    private readonly IMapper _mapper;
    private const int UserId = 10;

    public JobMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<JobMappingProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "JobMappingProfile - Configuração do mapeamento deve ser válida")]
    [Trait("Application", "")]
    public void ConfiguracaoMapeamento_DeveSerValida()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<JobMappingProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }

    #region JobDefinitionEntity -> JobResponse

    [Fact(DisplayName = "JobDefinitionEntity -> JobResponse - Deve mapear campos corretamente")]
    [Trait("Application", "")]
    public void JobDefinitionEntity_Para_JobResponse_DeveMapearCamposCorretamente()
    {
        var entity = BuildJob(3, "Financeiro", "Fechamento Mensal", "0 0 1 * *", 5, true);

        var result = _mapper.Map<JobResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Financeiro", result.JobCategory);
        Assert.Equal("Fechamento Mensal", result.JobName);
        Assert.Equal("0 0 1 * *", result.CronExpression);
        Assert.Equal(5, result.Priority);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "JobDefinitionEntity -> JobResponse - Deve mapear job inativo")]
    [Trait("Application", "")]
    public void JobDefinitionEntity_Para_JobResponse_DeveMapearJobInativo()
    {
        var entity = BuildJob(4, "Legado", "Job Legado", null, 1, false);

        var result = _mapper.Map<JobResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region JobDefinitionEntity -> JobDetailResponse

    [Fact(DisplayName = "JobDefinitionEntity -> JobDetailResponse - Deve mapear campos completos")]
    [Trait("Application", "")]
    public void JobDefinitionEntity_Para_JobDetailResponse_DeveMapearCamposCompletos()
    {
        var entity = BuildJob(5, "RH", "Folha de Pagamento", "0 8 5 * *", 3, true,
            description: "Processa folha mensal",
            purpose: "Pagamento dos funcionários",
            jobType: "Scheduled",
            method: "Execute",
            timeZoneId: "E. South America Standard Time",
            executeOnlyOnce: false,
            timeoutMinutes: 30,
            queue: "critical",
            maxRetries: 5,
            configuration: "{\"env\":\"prod\"}",
            isSystemJob: false);

        var result = _mapper.Map<JobDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Equal("RH", result.JobCategory);
        Assert.Equal("Folha de Pagamento", result.JobName);
        Assert.Equal("Processa folha mensal", result.Description);
        Assert.Equal("Pagamento dos funcionários", result.JobPurpose);
        Assert.Equal("Scheduled", result.JobType);
        Assert.Equal("Execute", result.JobMethod);
        Assert.Equal("0 8 5 * *", result.CronExpression);
        Assert.Equal("E. South America Standard Time", result.TimeZoneId);
        Assert.False(result.ExecuteOnlyOnce);
        Assert.Equal(30, result.TimeoutMinutes);
        Assert.Equal(3, result.Priority);
        Assert.Equal("critical", result.Queue);
        Assert.Equal(5, result.MaxRetries);
        Assert.Equal("{\"env\":\"prod\"}", result.JobConfiguration);
        Assert.False(result.IsSystemJob);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "JobDefinitionEntity -> JobDetailResponse - Deve mapear job inativo")]
    [Trait("Application", "")]
    public void JobDefinitionEntity_Para_JobDetailResponse_DeveMapearJobInativo()
    {
        var entity = BuildJob(6, "TI", "Backup", "0 2 * * *", 2, false);

        var result = _mapper.Map<JobDetailResponse>(entity);

        Assert.NotNull(result);
        Assert.Equal(6, result.Id);
        Assert.False(result.IsActive);
    }

    #endregion

    #region ListPage<JobDefinitionEntity> -> ListPageResponse<JobResponse>

    [Fact(DisplayName = "ListPage<JobDefinitionEntity> -> ListPageResponse<JobResponse> - Deve mapear paginação")]
    [Trait("Application", "")]
    public void ListPage_JobDefinitionEntity_Para_ListPageResponse_JobResponse_DeveMapear()
    {
        var items = new List<JobDefinitionEntity> { BuildJob(1, "Cat", "Job1", null, 5, true), BuildJob(2, "Cat", "Job2", null, 5, true) };
        var listPage = new ListPage<JobDefinitionEntity> { Items = items, TotalItems = 2, TotalPages = 1, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<JobResponse>>(listPage);

        Assert.NotNull(result);
    }

    [Fact(DisplayName = "ListPage<JobDefinitionEntity> vazio -> ListPageResponse<JobResponse> - Deve mapear lista vazia")]
    [Trait("Application", "")]
    public void ListPage_JobDefinitionEntity_Vazio_Para_ListPageResponse_JobResponse_DeveMapear()
    {
        var listPage = new ListPage<JobDefinitionEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };

        var result = _mapper.Map<ListPageResponse<JobResponse>>(listPage);

        Assert.NotNull(result);
    }

    #endregion

    private static JobDefinitionEntity BuildJob(
        int id,
        string category,
        string name,
        string? cronExpression,
        int priority,
        bool active,
        string description = null,
        string purpose = null,
        string jobType = "Recurring",
        string method = "Execute",
        string timeZoneId = "GMT Standard Time",
        bool executeOnlyOnce = false,
        int timeoutMinutes = 5,
        string queue = "default",
        int maxRetries = 3,
        string configuration = null,
        bool isSystemJob = false)
    {
        var entity = new JobDefinitionEntity(
            category, name, jobType, UserId,
            description: description,
            jobPurpose: purpose,
            jobMethod: method,
            cronExpression: cronExpression,
            timeZoneId: timeZoneId,
            executeOnlyOnce: executeOnlyOnce,
            timeoutMinutes: timeoutMinutes,
            priority: priority,
            queue: queue,
            maxRetries: maxRetries,
            jobConfiguration: configuration,
            isSystemJob: isSystemJob);

        typeof(VianaHub.Global.Identity.Domain.Base.Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);

        if (!active)
            entity.Deactivate(UserId);

        return entity;
    }
}
