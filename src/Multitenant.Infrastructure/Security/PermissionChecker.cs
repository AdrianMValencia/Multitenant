using Microsoft.EntityFrameworkCore;
using Multitenant.Application.Abstractions.Security;
using Multitenant.Infrastructure.Persistence.Context;

namespace Multitenant.Infrastructure.Security;

public class PermissionChecker(ApplicationDbContext context) : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission, CancellationToken cancellationToken = default)
    {
        var permissionParts = permission.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (permissionParts.Length != 2)
        {
            return false;
        }
        
        var resource = permissionParts[0];
        var action = permissionParts[1];

        return await(from ur in context.UserRoles
                     join rp in context.RolePermissions on ur.RoleId equals rp.RoleId
                     join p in context.Permissions on rp.PermissionId equals p.PermissionId
                     where ur.UserId == userId
                           && ur.TenantId == tenantId
                           && rp.TenantId == tenantId
                           && p.TenantId == tenantId
                           && p.Resource == resource
                           && p.Action == action
                     select p.PermissionId)
            .AnyAsync(cancellationToken);
    }
}
