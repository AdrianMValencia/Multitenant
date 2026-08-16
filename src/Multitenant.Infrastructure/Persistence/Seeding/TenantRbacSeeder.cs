using Microsoft.EntityFrameworkCore;
using Multitenant.Application.Abstractions.Security;
using Multitenant.Application.Abstractions.Multitenancy;
using Multitenant.Domain.Entities;
using Multitenant.Infrastructure.Persistence.Context;

namespace Multitenant.Infrastructure.Persistence.Seeding;

public class TenantRbacSeeder(ApplicationDbContext context, ITenantContext tenantContext) : ITenantRbacSeeder
{
    private readonly ApplicationDbContext _context = context;
    private readonly ITenantContext _tenantContext = tenantContext;

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
        // Establecer el tenant en el contexto para que las reglas de tenant se apliquen correctamente
        _tenantContext.SetTenant(tenantId);

        var tenant = await _context.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            // Crear tenant automáticamente en entorno de desarrollo para facilitar el seeding.
            tenant = new Tenant
            {
                Id = tenantId,
                Name = "Development Tenant",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var existingPermissions = await _context.Permissions
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var (resource, action) in DefaultPermissions)
        {
            if (existingPermissions.Any(x => x.Resource == resource && x.Action == action))
            {
                continue;
            }

            _context.Permissions.Add(new Permission
            {
                PermissionId = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"{resource}.{action}",
                Resource = resource,
                Action = action
            });
        }

        var adminRole = await _context.Roles
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

            _context.Roles.Add(adminRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var permissions = await _context.Permissions
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var rolePermissionIds = await _context.RolePermissions
            .Where(x => x.TenantId == tenantId && x.RoleId == adminRole.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in permissions)
        {
            if (rolePermissionIds.Contains(permission.PermissionId))
            {
                continue;
            }

            _context.RolePermissions.Add(new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                TenantId = tenantId,
                RoleId = adminRole.Id,
                PermissionId = permission.PermissionId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
