using Microsoft.AspNetCore.Http;
using Multitenant.Application.Multitenancy;

namespace Multitenant.Infrastructure.Multitenancy;

public class TenantResolutionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (!string.IsNullOrEmpty(tenantHeader) && Guid.TryParse(tenantHeader, out var tenantIdFromHeader))
        {
            tenantContext.SetTenant(tenantIdFromHeader);
        }
        else
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;

            if (!string.IsNullOrEmpty(tenantClaim) && Guid.TryParse(tenantClaim, out var tenantIdFromClaim))
            {
                tenantContext.SetTenant(tenantIdFromClaim);
            }
        }

        await _next(context);
    }
}
