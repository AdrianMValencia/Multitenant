namespace Multitenant.Application.Abstractions.Security;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission, 
        CancellationToken cancellationToken = default);
}
