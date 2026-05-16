using Microsoft.AspNetCore.Authorization;

namespace Multitenant.Api.Security;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"{PermissionPolicyProvider.PolicyPrefix}{permission}";
    }

    public string Permission { get; }
}
