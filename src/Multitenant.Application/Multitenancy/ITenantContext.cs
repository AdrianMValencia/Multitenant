namespace Multitenant.Application.Multitenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
    void SetTenant(Guid tenantId);
}
