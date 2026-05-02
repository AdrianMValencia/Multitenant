namespace Multitenant.Domain.Entities;

public class RolePermission : ITenantEntity
{
    public Guid RolePermissionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid? PermissionId { get; set; }
    public Tenant? Tenant { get; set; }
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}
