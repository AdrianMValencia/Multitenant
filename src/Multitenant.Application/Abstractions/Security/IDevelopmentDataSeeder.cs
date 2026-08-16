namespace Multitenant.Application.Abstractions.Security;

/// <summary>
/// Sembrado de datos de desarrollo: 2 empresas (tenants), admins y clientes de ejemplo.
/// Solo debe ejecutarse en Development al arrancar la API.
/// </summary>
public interface IDevelopmentDataSeeder
{
    /// <param name="cancellationToken">Permite abortar si el host se detiene.</param>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
