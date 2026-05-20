using VianaHub.Global.Identity.Domain.Base;

namespace VianaHub.Global.Identity.Domain.Entities;

public class JobDefinitionEntity : Entity, IAggregateRoot
{
    // Propriedades principais
    public string? HangfireJobId { get; private set; }
    public string? Category { get; private set; }
    public string? Type { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? Purpose { get; private set; }
    public string? CronExpression { get; private set; }
    public string? Configuration { get; private set; }
    public string? Method { get; private set; }
    public string? TimeZoneId { get; private set; }
    public bool ExecuteOnlyOnce { get; private set; }
    public int TimeoutMinutes { get; private set; }
    public int Priority { get; private set; }
    public string? Queue { get; private set; }
    public int MaxRetries { get; private set; }
    public bool IsSystemJob { get; private set; }
    public DateTime? LastRegisteredAt { get; private set; }

    // Estado lógico
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    // Construtor protegido para EF
    protected JobDefinitionEntity() { }

    // Fábrica de criação com valores padrão conforme regras de negócio
    public JobDefinitionEntity(
        string jobCategory,
        string jobName,
        string jobType,
        int createdBy,
        string description = null,
        string jobPurpose = null,
        string jobMethod = "Execute",
        string cronExpression = null,
        string timeZoneId = "GMT Standard Time",
        bool executeOnlyOnce = false,
        int timeoutMinutes = 5,
        int priority = 5,
        string queue = "default",
        int maxRetries = 3,
        string jobConfiguration = null,
        bool isSystemJob = false)
        : base()
    {
        Category = jobCategory;
        Name = jobName;
        Description = description;
        Purpose = jobPurpose;
        Type = jobType;
        Method = string.IsNullOrWhiteSpace(jobMethod) ? "Execute" : jobMethod;
        CronExpression = cronExpression;
        TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "GMT Standard Time" : timeZoneId;
        ExecuteOnlyOnce = executeOnlyOnce;
        TimeoutMinutes = timeoutMinutes;
        Priority = priority;
        Queue = string.IsNullOrWhiteSpace(queue) ? "default" : queue;
        MaxRetries = maxRetries;
        Configuration = jobConfiguration;
        IsSystemJob = isSystemJob;

        IsActive = true;
        IsDeleted = false;

        AddedBy = createdBy;
        AddedOn = DateTime.UtcNow;
    }

    public void Update(
        string? description,
        string? jobPurpose,
        string? cronExpression,
        string? timeZoneId,
        int timeoutMinutes,
        int priority,
        string? queue,
        int maxRetries,
        string? jobConfiguration,
        bool isActive,
        int? modifiedBy)
    {
        Description = description;
        Purpose = jobPurpose;
        CronExpression = cronExpression;
        TimeZoneId = timeZoneId;
        TimeoutMinutes = timeoutMinutes;
        Priority = priority;
        Queue = queue;
        MaxRetries = maxRetries;
        Configuration = jobConfiguration;
        IsActive = isActive;

        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Activate(int? modifiedBy)
    {
        IsActive = true;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate(int? modifiedBy)
    {
        IsActive = false;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Delete(int? modifiedBy)
    {
        if (IsSystemJob)
            throw new InvalidOperationException("Cannot delete system job");

        IsDeleted = true;
        IsActive = false;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetHangfireRegistration(string hangfireJobId)
    {
        HangfireJobId = hangfireJobId;
        LastRegisteredAt = DateTime.UtcNow;
    }
}
