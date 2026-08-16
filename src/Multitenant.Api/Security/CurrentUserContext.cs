using Multitenant.Application.Abstractions.Security;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;

namespace Multitenant.Api.Security;

public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => TryParseGuid(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue(ClaimTypes.Name) ?? User?.FindFirstValue("sub"));

    public Guid? TenantId => TryParseGuid(User?.FindFirstValue("tenant_id"));

    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");

    public IReadOnlyCollection<string> Roles => User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray() ?? Array.Empty<string>();

    private static Guid? TryParseGuid(string? value)
        => Guid.TryParse(value, out var id) ? id : null;
}
