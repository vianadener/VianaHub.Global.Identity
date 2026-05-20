using VianaHub.Global.Identity.Domain.Base;

namespace VianaHub.Global.Identity.Domain.Entities;

/// <summary>
/// Entidade que representa um recurso do sistema
/// </summary>
public class ResourceEntity : Entity
{
    public int TenantId { get; private set; }
    public int AppId { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation Properties
    public TenantEntity? Tenant { get; private set; }
    public AppEntity? App { get; private set; }

    private readonly List<RolePermissionEntity> _rolePermissions = new();
    public IReadOnlyCollection<RolePermissionEntity> RolePermissions => _rolePermissions.AsReadOnly();

    // Construtor protegido para o EF Core
    protected ResourceEntity() { }

    /// <summary>
    /// Construtor para criação de um novo recurso
    /// </summary>
    public ResourceEntity(int tenantId, int appId, string name, string description, int createdBy)
    {
        TenantId = tenantId;
        AppId = appId;
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
