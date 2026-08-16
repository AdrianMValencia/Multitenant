using Microsoft.AspNetCore.Authorization;
using Multitenant.Application.Abstractions.Security;

namespace Multitenant.Api.Security;

public class PermissionAuthorizationHandler(
 ICurrentUserContext currentUserContext,
 IPermissionChecker permissionChecker) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (currentUserContext.UserId is null || currentUserContext.TenantId is null)
        {
            return;
        }

        if (await permissionChecker.HasPermissionAsync(currentUserContext.UserId.Value, currentUserContext.TenantId.Value, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}

