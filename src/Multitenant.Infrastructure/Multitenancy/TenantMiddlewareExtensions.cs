using Microsoft.AspNetCore.Builder;

namespace Multitenant.Infrastructure.Multitenancy;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
