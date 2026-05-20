using VianaHub.Global.Identity.Domain.Base;

namespace VianaHub.Global.Identity.Domain.Entities;

/// <summary>
/// Entidade que representa um Tenant (inquilino) no sistema multi-tenant.
/// Aggregate Root para o contexto de Tenant.
/// </summary>
public class TenantEntity : Entity, IAggregateRoot
{
    private readonly List<UserEntity> _users = [];

    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? Alias { get; private set; }
    public string? UrlImage { get; private set; }
    public string? Settings { get; private set; }
    public string? Remarks { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    // Partes internas do agregado
    public IReadOnlyCollection<UserEntity> Users => _users.AsReadOnly();

    // Construtor protegido para o EF Core
    protected TenantEntity() { }

    /// <summary>
    /// Construtor para criação de um novo Tenant
    /// </summary>
    public TenantEntity(string name, string description, string alias, string? urlImage, string? settings, string? remarks, int createdBy)
    {
        Name = name;
        Description = description;
        Alias = alias;
        UrlImage = urlImage;
        Settings = settings;
        Remarks = remarks;
        IsActive = true;
        IsDeleted = false;
        AddedBy = createdBy;
        AddedOn = DateTime.UtcNow;
    }

    public void Update(string name, string description, string alias, string? urlImage, string? settings, string? remarks, int modifiedBy)
    {
        Name = name;
        Description = description;
        Alias = alias;
        UrlImage = urlImage;
        Settings = settings;
        Remarks = remarks;
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
