using Microsoft.AspNetCore.Authorization;

namespace Multitenant.Api.Security;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
