namespace VianaHub.Global.Identity.Domain.Entities;

/// <summary>
/// Entidade que representa a permissão de uma Role sobre um Resource com uma ActionEntity
/// </summary>
public class RolePermissionEntity
{
    public int Id { get; private set; }
    public int TenantId { get; private set; }
    public int AppId { get; private set; }
    public int RoleId { get; private set; }
    public int ResourceId { get; private set; }
    public int ActionId { get; private set; }

    // Navigation Properties
    public TenantEntity? Tenant { get; private set; }
    public AppEntity? App { get; private set; }
    public RoleEntity? Role { get; private set; }
    public ResourceEntity? Resource { get; private set; }
    public ActionEntity? Action { get; private set; }

    // Construtor protegido para o EF Core
    protected RolePermissionEntity() { }

    /// <summary>
    /// Construtor para criação de uma nova permissão de Role
    /// </summary>
    public RolePermissionEntity(int tenantId, int appId, int roleId, int resourceId, int actionId)
    {
        TenantId = tenantId;
        AppId = appId;
        RoleId = roleId;
        ResourceId = resourceId;
        ActionId = actionId;
    }
}
