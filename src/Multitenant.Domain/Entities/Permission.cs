namespace Multitenant.Domain.Entities;

public class Permission : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? Name { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
