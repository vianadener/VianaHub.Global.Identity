using VianaHub.Global.Identity.Domain.Interfaces.Base;

namespace VianaHub.Global.Identity.Infra.Data.Context;

public sealed class RequestTenantContext : IRequestTenantContext
{
    private int? _tenantId;

    public int? TenantId => _tenantId;

    public void SetTenantId(int tenantId) => _tenantId = tenantId;

    public void Clear() => _tenantId = null;
}
