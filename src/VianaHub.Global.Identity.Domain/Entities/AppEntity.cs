using VianaHub.Global.Identity.Domain.Base;

namespace VianaHub.Global.Identity.Domain.Entities;

/// <summary>
/// Entidade que representa uma aplicação no sistema
/// </summary>
public class AppEntity : Entity
{
    public int TenantId { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation Properties
    public TenantEntity? Tenant { get; private set; }

    // Construtor protegido para o EF Core
    protected AppEntity() { }

    /// <summary>
    /// Construtor para criação de uma nova aplicação
    /// </summary>
    public AppEntity(int tenantId, string name, string description, int createdBy)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        IsActive = true;
        IsDeleted = false;
        AddedBy = createdBy;
    }

    public void Update(string name, string description, int modifiedBy)
    {
        Name = name;
        Description = description;
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
        IsDeleted = true;
        IsActive = false;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }
}
