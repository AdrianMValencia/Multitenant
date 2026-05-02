namespace Multitenant.Domain.Entities;

public class UserRole : ITenantEntity
{
    public Guid UserRoleId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }

    public Tenant? Tenant { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }
}
