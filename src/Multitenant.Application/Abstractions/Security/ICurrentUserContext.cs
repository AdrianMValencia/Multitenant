namespace Multitenant.Application.Abstractions.Security;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
}
