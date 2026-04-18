using Multitenant.Application.Multitenancy;

namespace Multitenant.Infrastructure.Multitenancy;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
