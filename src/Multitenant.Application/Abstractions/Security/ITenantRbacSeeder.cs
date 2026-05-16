namespace Multitenant.Application.Abstractions.Security;

public interface ITenantRbacSeeder
{
    Task SeedAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
