using AutoMapper;
using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Job;
using VianaHub.Global.Identity.Application.Dto.Response.Job;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Application.Services;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Identity.Domain.Tools.Pagination;
using FluentValidation.Results;
using Moq;

namespace VianaHub.Global.Identity.Tests.Application;

public class JobAppServiceTests
{
    private readonly Mock<IJobDefinitionDataRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<INotify> _notifyMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IEntityDomainValidator<JobDefinitionEntity>> _validatorMock = new();
    private readonly Mock<IJobSchedulerService> _schedulerMock = new();

    private const int UserId = 10;

    public JobAppServiceTests()
    {
        _currentUserMock.Setup(x => x.GetUserId()).Returns(UserId);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>())).Returns<string>(k => k);
        _localizationMock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>())).Returns<string, object[]>((k, _) => k);
    }

    private JobAppService CreateSut() => new(
        _repoMock.Object,
        _mapperMock.Object,
        _notifyMock.Object,
        _localizationMock.Object,
        _currentUserMock.Object,
        _validatorMock.Object,
        _schedulerMock.Object);

    private static JobDefinitionEntity BuildJob(int id = 1, string name = "TestJob", bool active = true, bool executeOnlyOnce = false, string hangfireId = null)
    {
        var entity = new JobDefinitionEntity("Category", name, "JobType", UserId, "Description");
        typeof(Entity).GetProperty("Id")!.SetValue(entity, id);

        if (!active)
            entity.Deactivate(UserId);

        if (!string.IsNullOrWhiteSpace(hangfireId))
            entity.SetHangfireRegistration(hangfireId);

        return entity;
    }

    private static ValidationResult ValidResult() => new();
    private static ValidationResult InvalidResult(string error = "Erro de validação") =>
        new(new[] { new ValidationFailure("Field", error) });

    #region GetAllAsync

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista de jobs mapeados")]
    [Trait("Application", "")]
    public async Task GetAllAsync_Sucesso_DeveRetornarLista()
    {
        var entities = new List<JobDefinitionEntity> { BuildJob(1), BuildJob(2) };
        var mapped = new List<JobResponse> { new(1, "Cat", "Job1", "* * * * *", 5, true), new(2, "Cat", "Job2", "* * * * *", 5, true) };
        _repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync(entities);
        _mapperMock.Setup(x => x.Map<IEnumerable<JobResponse>>(entities)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "GetAllAsync - Deve retornar lista vazia quando não há jobs")]
    [Trait("Application", "")]
    public async Task GetAllAsync_ListaVazia_DeveRetornarVazio()
    {
        _repoMock.Setup(x => x.GetAllAsync(default)).ReturnsAsync([]);
        _mapperMock.Setup(x => x.Map<IEnumerable<JobResponse>>(It.IsAny<IEnumerable<JobDefinitionEntity>>())).Returns([]);

        var sut = CreateSut();
        var result = await sut.GetAllAsync(default);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = "GetByIdAsync - Deve retornar job quando encontrado")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_Sucesso_DeveRetornarJob()
    {
        var entity = BuildJob(1);
        var mapped = new JobDetailResponse(1, 1, "Tenant", "Cat", "TestJob", "Desc", "Purpose", "Type", "Execute", "* * * * *", "GMT Standard Time", false, 5, 5, "default", 3, "Config", false, null, null, null, null, "OK", 1, true);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _mapperMock.Setup(x => x.Map<JobDetailResponse>(entity)).Returns(mapped);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(1, default);

        Assert.NotNull(result);
        Assert.IsType<JobDetailResponse>(result);
    }

    [Fact(DisplayName = "GetByIdAsync - Deve retornar null e notificar quando job não encontrado")]
    [Trait("Application", "")]
    public async Task GetByIdAsync_NaoEncontrado_DeveRetornarNullENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JobDefinitionEntity)null);

        var sut = CreateSut();
        var result = await sut.GetByIdAsync(99, default);

        Assert.Null(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
    }

    #endregion

    #region GetPagedAsync

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página de jobs")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_Sucesso_DeveRetornarPaginado()
    {
        var entities = new List<JobDefinitionEntity> { BuildJob(1) };
        var listPage = new ListPage<JobDefinitionEntity> { Items = entities, TotalItems = 1, TotalPages = 1, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<JobResponse>(new List<JobResponse> { new(1, "Cat", "Job1", "* * * * *", 5, true) }, 1, 10, 1, 1);
        _repoMock.Setup(x => x.GetPagedAsync(It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<JobResponse>>(listPage)).Returns(mappedPage);

        var request = new JobPagedFilter { Search = "", PageNumber = 1, PageSize = 10, SortBy = "Name", SortDirection = "asc" };

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(request, default);

        Assert.NotNull(result);
        Assert.Single(result.Items);
    }

    [Fact(DisplayName = "GetPagedAsync - Deve retornar página vazia quando não há registros")]
    [Trait("Application", "")]
    public async Task GetPagedAsync_ListaVazia_DeveRetornarPaginadoVazio()
    {
        var listPage = new ListPage<JobDefinitionEntity> { Items = [], TotalItems = 0, TotalPages = 0, PageNumber = 1, PageSize = 10 };
        var mappedPage = new ListPageResponse<JobResponse>([], 1, 10, 0, 0);
        _repoMock.Setup(x => x.GetPagedAsync(It.IsAny<PagedFilter>(), default)).ReturnsAsync(listPage);
        _mapperMock.Setup(x => x.Map<ListPageResponse<JobResponse>>(listPage)).Returns(mappedPage);

        var request = new JobPagedFilter { Search = "", PageNumber = 1, PageSize = 10, SortBy = "Name", SortDirection = "asc" };

        var sut = CreateSut();
        var result = await sut.GetPagedAsync(request, default);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    #endregion

    #region CreateAsync

    [Fact(DisplayName = "CreateAsync - Deve criar job com sucesso")]
    [Trait("Application", "")]
    public async Task CreateAsync_Sucesso_DeveRetornarTrue()
    {
        var request = new CreateJobRequest("Cat", "NovoJob", "", "", "Type", "", "* * * * *");
        _repoMock.Setup(x => x.ExistsByNameAsync(request.JobName, default)).ReturnsAsync(false);
        _validatorMock.Setup(x => x.ValidateForCreateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.CreateAsync(It.IsAny<JobDefinitionEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.True(result);
        _repoMock.Verify(x => x.CreateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false e notificar quando nome já existe")]
    [Trait("Application", "")]
    public async Task CreateAsync_NomeJaExiste_DeveRetornarFalseENotificar()
    {
        var request = new CreateJobRequest("Cat", "JobExistente", "", "", "Type", "", "* * * * *");
        _repoMock.Setup(x => x.ExistsByNameAsync(request.JobName, default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 409), Times.Once);
        _repoMock.Verify(x => x.CreateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false quando validação de domínio falha")]
    [Trait("Application", "")]
    public async Task CreateAsync_ValidacaoDominioFalha_DeveRetornarFalseENotificar()
    {
        var request = new CreateJobRequest("Cat", "NovoJob", "", "", "Type", "", "* * * * *");
        _repoMock.Setup(x => x.ExistsByNameAsync(request.JobName, default)).ReturnsAsync(false);
        _validatorMock.Setup(x => x.ValidateForCreateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _repoMock.Verify(x => x.CreateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "CreateAsync - Deve retornar false quando repositório falha")]
    [Trait("Application", "")]
    public async Task CreateAsync_RepositorioFalha_DeveRetornarFalse()
    {
        var request = new CreateJobRequest("Cat", "NovoJob", "", "", "Type", "", "* * * * *");
        _repoMock.Setup(x => x.ExistsByNameAsync(request.JobName, default)).ReturnsAsync(false);
        _validatorMock.Setup(x => x.ValidateForCreateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(ValidResult());
        _repoMock.Setup(x => x.CreateAsync(It.IsAny<JobDefinitionEntity>(), default)).ReturnsAsync(false);

        var sut = CreateSut();
        var result = await sut.CreateAsync(request, default);

        Assert.False(result);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = "UpdateAsync - Deve atualizar job com sucesso (job ativo e recorrente)")]
    [Trait("Application", "")]
    public async Task UpdateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildJob(1);
        var request = new UpdateJobRequest("Nova Desc", "", "0 * * * *", "GMT Standard Time", 5, 5, "default", 3, "", true);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForUpdateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(ValidResult());
        _schedulerMock.Setup(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(1, request, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "UpdateAsync - Deve remover do Hangfire quando job é desativado")]
    [Trait("Application", "")]
    public async Task UpdateAsync_DesativarJob_DeveRemoverDoHangfire()
    {
        var entity = BuildJob(1, hangfireId: "job-1");
        var request = new UpdateJobRequest("Desc", "", "", "GMT Standard Time", 5, 5, "default", 3, "", false);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForUpdateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(ValidResult());
        _schedulerMock.Setup(x => x.RemoveRecurringAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(1, request, default);

        Assert.True(result);
        _schedulerMock.Verify(x => x.RemoveRecurringAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact(DisplayName = "UpdateAsync - Deve retornar false e notificar quando job não encontrado")]
    [Trait("Application", "")]
    public async Task UpdateAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JobDefinitionEntity)null);
        var request = new UpdateJobRequest("Desc", "", "", "GMT Standard Time", 5, 5, "default", 3, "", true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(99, request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "UpdateAsync - Deve retornar false quando validação de domínio falha")]
    [Trait("Application", "")]
    public async Task UpdateAsync_ValidacaoDominioFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildJob(1);
        var request = new UpdateJobRequest("Desc", "", "", "GMT Standard Time", 5, 5, "default", 3, "", true);

        var sut = CreateSut();
        var result = await sut.UpdateAsync(1, request, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    #endregion

    #region ActivateAsync

    [Fact(DisplayName = "ActivateAsync - Deve ativar job recorrente e registrar no Hangfire")]
    [Trait("Application", "")]
    public async Task ActivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildJob(1, active: false);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForActivateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(ValidResult());
        _schedulerMock.Setup(x => x.RegisterRecurringAsync(It.IsAny<JobDefinitionEntity>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(1, default);

        Assert.True(result);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "ActivateAsync - Deve retornar false e notificar quando job não encontrado")]
    [Trait("Application", "")]
    public async Task ActivateAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JobDefinitionEntity)null);

        var sut = CreateSut();
        var result = await sut.ActivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "ActivateAsync - Deve retornar false quando validação de domínio falha")]
    [Trait("Application", "")]
    public async Task ActivateAsync_ValidacaoDominioFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildJob(1, active: false);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForActivateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.ActivateAsync(1, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    #endregion

    #region DeactivateAsync

    [Fact(DisplayName = "DeactivateAsync - Deve desativar job e remover do Hangfire")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildJob(1, hangfireId: "job-1");
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForDeactivateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(ValidResult());
        _schedulerMock.Setup(x => x.RemoveRecurringAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(1, default);

        Assert.True(result);
        _schedulerMock.Verify(x => x.RemoveRecurringAsync(It.IsAny<string>()), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false e notificar quando job não encontrado")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JobDefinitionEntity)null);

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "DeactivateAsync - Deve retornar false quando validação de domínio falha")]
    [Trait("Application", "")]
    public async Task DeactivateAsync_ValidacaoDominioFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildJob(1, hangfireId: "job-1");
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForDeactivateAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeactivateAsync(1, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = "DeleteAsync - Deve excluir job e remover do Hangfire")]
    [Trait("Application", "")]
    public async Task DeleteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildJob(1, hangfireId: "job-1");
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(ValidResult());
        _schedulerMock.Setup(x => x.RemoveRecurringAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _repoMock.Setup(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default)).ReturnsAsync(true);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(1, default);

        Assert.True(result);
        _schedulerMock.Verify(x => x.RemoveRecurringAsync(It.IsAny<string>()), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Once);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false e notificar quando job não encontrado")]
    [Trait("Application", "")]
    public async Task DeleteAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JobDefinitionEntity)null);

        var sut = CreateSut();
        var result = await sut.DeleteAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    [Fact(DisplayName = "DeleteAsync - Deve retornar false quando validação de domínio falha")]
    [Trait("Application", "")]
    public async Task DeleteAsync_ValidacaoDominioFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildJob(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _validatorMock.Setup(x => x.ValidateForDeleteAsync(It.IsAny<JobDefinitionEntity>())).ReturnsAsync(InvalidResult());

        var sut = CreateSut();
        var result = await sut.DeleteAsync(1, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.AtLeastOnce);
        _repoMock.Verify(x => x.UpdateAsync(It.IsAny<JobDefinitionEntity>(), default), Times.Never);
    }

    #endregion

    #region ExecuteAsync

    [Fact(DisplayName = "ExecuteAsync - Deve enfileirar job com sucesso")]
    [Trait("Application", "")]
    public async Task ExecuteAsync_Sucesso_DeveRetornarTrue()
    {
        var entity = BuildJob(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _schedulerMock.Setup(x => x.EnqueueJobAsync(entity)).ReturnsAsync("job-enqueued-id");

        var sut = CreateSut();
        var result = await sut.ExecuteAsync(1, default);

        Assert.True(result);
        _schedulerMock.Verify(x => x.EnqueueJobAsync(entity), Times.Once);
    }

    [Fact(DisplayName = "ExecuteAsync - Deve retornar false e notificar quando job não encontrado")]
    [Trait("Application", "")]
    public async Task ExecuteAsync_NaoEncontrado_DeveRetornarFalseENotificar()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, default)).ReturnsAsync((JobDefinitionEntity)null);

        var sut = CreateSut();
        var result = await sut.ExecuteAsync(99, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 410), Times.Once);
        _schedulerMock.Verify(x => x.EnqueueJobAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "ExecuteAsync - Deve retornar false e notificar quando job está inativo")]
    [Trait("Application", "")]
    public async Task ExecuteAsync_JobInativo_DeveRetornarFalseENotificar()
    {
        var entity = BuildJob(1, active: false);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);

        var sut = CreateSut();
        var result = await sut.ExecuteAsync(1, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
        _schedulerMock.Verify(x => x.EnqueueJobAsync(It.IsAny<JobDefinitionEntity>()), Times.Never);
    }

    [Fact(DisplayName = "ExecuteAsync - Deve retornar false e notificar quando scheduler falha ao enfileirar")]
    [Trait("Application", "")]
    public async Task ExecuteAsync_SchedulerFalha_DeveRetornarFalseENotificar()
    {
        var entity = BuildJob(1);
        _repoMock.Setup(x => x.GetByIdAsync(1, default)).ReturnsAsync(entity);
        _schedulerMock.Setup(x => x.EnqueueJobAsync(entity)).ReturnsAsync(string.Empty);

        var sut = CreateSut();
        var result = await sut.ExecuteAsync(1, default);

        Assert.False(result);
        _notifyMock.Verify(x => x.Add(It.IsAny<string>(), 400), Times.Once);
    }

    #endregion
}
