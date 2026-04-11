namespace Multitenant.Domain.Entities;

public class Tenant : BaseEntity
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Email { get; set; }
    public string? Plan { get; set; }
    public bool? IsActive { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Permission> Permissions { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
}
