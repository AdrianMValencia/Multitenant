using Microsoft.EntityFrameworkCore;
using Multitenant.Application.Abstractions.Security;
using Multitenant.Domain.Entities;
using Multitenant.Infrastructure.Persistence.Context;

namespace Multitenant.Infrastructure.Persistence.Seeding;

public class TenantRbacSeeder(ApplicationDbContext context) : ITenantRbacSeeder
{
    private static readonly (string Resource, string Action)[] DefaultPermissions =
    [
        ("customers", "read"),
        ("customers", "write"),
        ("projects", "read"),
        ("projects", "write"),
        ("sales", "read"),
        ("sales", "write"),
        ("dashboards", "read"),
        ("dashboards", "write")
    ];

    public async Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenantExists = await context.Tenants.AnyAsync(x => x.Id == tenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new InvalidOperationException("El tenant indicado no existe.");
        }

        var existingPermissions = await context.Permissions
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var (resource, action) in DefaultPermissions)
        {
            if (existingPermissions.Any(x => x.Resource == resource && x.Action == action))
            {
                continue;
            }

            context.Permissions.Add(new Permission
            {
                PermissionId = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"{resource}.{action}",
                Resource = resource,
                Action = action
            });
        }

        var adminRole = await context.Roles
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name == "Admin", cancellationToken);

        if (adminRole is null)
        {
            adminRole = new Role
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = "Admin",
                Description = "Administrador del tenant",
                CreatedAt = DateTime.UtcNow
            };

            context.Roles.Add(adminRole);
            await context.SaveChangesAsync(cancellationToken);
        }

        var permissions = await context.Permissions
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var rolePermissionIds = await context.RolePermissions
            .Where(x => x.TenantId == tenantId && x.RoleId == adminRole.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            if (rolePermissionIds.Contains(permission.PermissionId))
            {
                continue;
            }

            context.RolePermissions.Add(new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = adminRole.Id,
                PermissionId = permission.PermissionId
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
