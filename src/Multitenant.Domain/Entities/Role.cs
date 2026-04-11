namespace Multitenant.Domain.Entities;

public class Role : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
