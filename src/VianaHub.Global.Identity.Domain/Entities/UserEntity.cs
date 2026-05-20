using VianaHub.Global.Identity.Domain.Base;

namespace VianaHub.Global.Identity.Domain.Entities;

/// <summary>
/// Entidade que representa um usuário do sistema
/// </summary>
public class UserEntity : Entity
{
    public int TenantId { get; private set; }
    public string? Name { get; private set; }
    public string? LoginIdentifier { get; private set; }
    public string? NormalizedLoginIdentifier { get; private set; }
    public string? PasswordHash { get; private set; }
    public string? UrlImage { get; private set; }
    public DateTime? LastAccessAt { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation Properties
    public TenantEntity? Tenant { get; private set; }

    private readonly List<UserRoleEntity> _userRoles = new();
    public IReadOnlyCollection<UserRoleEntity> UserRoles => _userRoles.AsReadOnly();

    // Construtor protegido para o EF Core
    protected UserEntity() { }

    /// <summary>
    /// Construtor para criação de um novo usuário
    /// </summary>
    public UserEntity(int tenantId, string name, string loginIdentifier, string passwordHash, string? urlImage, int createdBy)
    {
        TenantId = tenantId;
        Name = name;
        LoginIdentifier = loginIdentifier;
        NormalizedLoginIdentifier = loginIdentifier?.ToUpperInvariant();
        PasswordHash = passwordHash;
        UrlImage = urlImage;
        IsActive = true;
        IsDeleted = false;
        AddedBy = createdBy;
    }

    public void Update(string name, string? urlImage, int modifiedBy)
    {
        Name = name;
        UrlImage = urlImage;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash, int modifiedBy)
    {
        PasswordHash = passwordHash;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateLoginIdentifier(string loginIdentifier, int modifiedBy)
    {
        LoginIdentifier = loginIdentifier;
        NormalizedLoginIdentifier = loginIdentifier?.ToUpperInvariant();
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateLastAccess()
    {
        LastAccessAt = DateTime.UtcNow;
    }

    public void Activate(int modifiedBy)
    {
        IsActive = true;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate(int modifiedBy)
    {
        IsActive = false;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Delete(int modifiedBy)
    {
        IsDeleted = true;
        IsActive = false;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

}
