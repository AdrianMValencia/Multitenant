namespace Multitenant.Application.Abstractions.Multitenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
    void SetTenant(Guid tenantId);
}
