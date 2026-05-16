using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Multitenant.Api.Security;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public const string PolicyPrefix = "Permission:";

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}
