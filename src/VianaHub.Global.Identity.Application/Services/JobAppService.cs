using AutoMapper;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Job;
using VianaHub.Global.Identity.Application.Dto.Response.Job;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.Base;
using VianaHub.Global.Identity.Domain.Entities;
using VianaHub.Global.Identity.Domain.Interfaces;
using VianaHub.Global.Identity.Domain.Interfaces.Base;
using VianaHub.Global.Identity.Domain.ReadModels;
using VianaHub.Global.Middleware.Lib.Notifications;

namespace VianaHub.Global.Identity.Application.Services;

public class JobAppService : IJobAppService
{
    private readonly IJobDefinitionDataRepository _repo;
    private readonly IMapper _mapper;
    private readonly INotify _notify;
    private readonly ILocalizationService _localization;
    private readonly ICurrentUserService _currentUser;
    private readonly IEntityDomainValidator<JobDefinitionEntity> _validator;
    private readonly IJobSchedulerService _scheduler;

    public JobAppService(
        IJobDefinitionDataRepository repo,
        IMapper mapper,
        INotify notify,
        ILocalizationService localization,
        ICurrentUserService currentUser,
        IEntityDomainValidator<JobDefinitionEntity> validator,
        IJobSchedulerService scheduler)
    {
        _repo = repo;
        _mapper = mapper;
        _notify = notify;
        _localization = localization;
        _currentUser = currentUser;
        _validator = validator;
        _scheduler = scheduler;
    }

    public async Task<IEnumerable<JobResponse>> GetAllAsync(CancellationToken ct)
    {
        var entities = await _repo.GetAllAsync(ct);
        return _mapper.Map<IEnumerable<JobResponse>>(entities);
    }

    public async Task<JobDetailResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.GetById.Gone"), 410);
            return null;
        }

        return _mapper.Map<JobDetailResponse>(entity);
    }

    public async Task<ListPageResponse<JobResponse>> GetPagedAsync(JobPagedFilter request, CancellationToken ct)
    {
        var paged = await _repo.GetPagedAsync(request, ct);
        return _mapper.Map<ListPageResponse<JobResponse>>(paged);
    }

    public async Task<bool> CreateAsync(CreateJobRequest request, CancellationToken ct)
    {
        // Unicidade
        var exists = await _repo.ExistsByNameAsync(request.JobName, ct);
        if (exists)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Create.ResourceAlreadyExists"), 409);
            return false;
        }

        var entity = new JobDefinitionEntity(
            request.JobCategory,
            request.JobName,
            request.JobType,
            _currentUser.GetUserId(),
            request.Description,
            request.JobPurpose,
            request.JobMethod,
            request.CronExpression,
            request.TimeZoneId,
            request.ExecuteOnlyOnce,
            request.TimeoutMinutes,
            request.Priority,
            request.Queue,
            request.MaxRetries,
            request.JobConfiguration,
            request.IsSystemJob
        );

        // Validação de domínio
        var vr = await _validator.ValidateForCreateAsync(entity);
        if (!vr.IsValid)
        {
            foreach (var error in vr.Errors)
                _notify.Add(error.ErrorMessage, 400);
            return false;
        }

        return await _repo.CreateAsync(entity, ct);
    }

    public async Task<bool> UpdateAsync(int id, UpdateJobRequest request, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Update.ResourceNotFound"), 410);
            return false;
        }

        var oldHangfireId = entity.HangfireJobId;

        // Não permitir alterar JobName ou IsSystemJob
        // Campos permitidos são atualizados via método Update na entidade
        entity.Update(
            request.Description,
            request.JobPurpose,
            request.CronExpression,
            request.TimeZoneId,
            request.TimeoutMinutes,
            request.Priority,
            request.Queue,
            request.MaxRetries,
            request.JobConfiguration,
            request.IsActive,
            _currentUser.GetUserId()
        );

        var vr = await _validator.ValidateForUpdateAsync(entity);
        if (!vr.IsValid)
        {
            foreach (var error in vr.Errors)
                _notify.Add(error.ErrorMessage, 400);
            return false;
        }

        // If job remains active and is recurring, re-register
        if (entity.IsActive && !entity.ExecuteOnlyOnce)
        {
            await _scheduler.RegisterRecurringAsync(entity);
            entity.SetHangfireRegistration(entity.Name);
        }
        else if (!entity.IsActive && !string.IsNullOrWhiteSpace(oldHangfireId))
        {
            await _scheduler.RemoveRecurringAsync(oldHangfireId);
        }

        return await _repo.UpdateAsync(entity, ct);
    }

    public async Task<bool> ActivateAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Activate.ResourceNotFound"), 410);
            return false;
        }

        entity.Activate(_currentUser.GetUserId());
        var vr = await _validator.ValidateForActivateAsync(entity);
        if (!vr.IsValid)
        {
            foreach (var error in vr.Errors)
                _notify.Add(error.ErrorMessage, 400);
            return false;
        }

        // Register in Hangfire if recurring
        if (!entity.ExecuteOnlyOnce)
        {
            await _scheduler.RegisterRecurringAsync(entity);
            entity.SetHangfireRegistration(entity.Name);
        }

        return await _repo.UpdateAsync(entity, ct);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Deactivate.ResourceNotFound"), 410);
            return false;
        }

        var oldHangfireId = entity.HangfireJobId;

        entity.Deactivate(_currentUser.GetUserId());
        var vr = await _validator.ValidateForDeactivateAsync(entity);
        if (!vr.IsValid)
        {
            foreach (var error in vr.Errors)
                _notify.Add(error.ErrorMessage, 400);
            return false;
        }

        // Remove from Hangfire if it was registered
        if (!string.IsNullOrWhiteSpace(oldHangfireId))
        {
            await _scheduler.RemoveRecurringAsync(oldHangfireId);
        }

        return await _repo.UpdateAsync(entity, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Delete.ResourceNotFound"), 410);
            return false;
        }

        entity.Delete(_currentUser.GetUserId());
        var vr = await _validator.ValidateForDeleteAsync(entity);
        if (!vr.IsValid)
        {
            foreach (var error in vr.Errors)
                _notify.Add(error.ErrorMessage, 400);
            return false;
        }

        // Remove from Hangfire if it was registered
        if (!string.IsNullOrWhiteSpace(entity.HangfireJobId))
        {
            await _scheduler.RemoveRecurringAsync(entity.HangfireJobId);
        }

        return await _repo.UpdateAsync(entity, ct);
    }

    public async Task<bool> ExecuteAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Execute.ResourceNotFound"), 410);
            return false;
        }

        if (!entity.IsActive)
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Execute.JobNotActive"), 400);
            return false;
        }

        // Enqueue job via scheduler
        var jobId = await _scheduler.EnqueueJobAsync(entity);
        if (string.IsNullOrWhiteSpace(jobId))
        {
            _notify.Add(_localization.GetMessage("Application.Service.Job.Execute.FailedToEnqueue"), 400);
            return false;
        }

        return true;
    }
}
