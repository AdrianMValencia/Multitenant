using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Multitenant.Application.Abstractions.Multitenancy;

namespace Multitenant.Infrastructure.Persistence.Context;

/// <summary>
/// Factory para crear ApplicationDbContext en tiempo de diseño (migraciones).
/// Evita resolver dependencias complejas como CurrentUserContext.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Configurar builder de opciones
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Obtener la configuración del proyecto Api de manera robusta
        var basePath = GetApiBasePath();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: false, reloadOnChange: true)
            .AddJsonFile(Path.Combine(basePath, "appsettings.Development.json"), optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("MultitenantConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("No se encontró la cadena de conexión 'MultitenantConnection' en appsettings.json");
        }

        optionsBuilder.UseNpgsql(connectionString);

        // Crear un TenantContext dummy para la migración
        var tenantContext = new DesignTimeTenantContext();

        return new ApplicationDbContext(optionsBuilder.Options, configuration, tenantContext);
    }

    private static string GetApiBasePath()
    {
        // Intenta buscar la carpeta Api en la ruta del directorio actual
        var currentDir = Directory.GetCurrentDirectory();
        var apiPath = Path.Combine(currentDir, "src", "Multitenant.Api");

        if (Directory.Exists(apiPath))
            return apiPath;

        // Si no encuentra en esa ruta, intenta ir hacia atrás
        apiPath = Path.Combine(currentDir, "..", "Multitenant.Api");
        if (Directory.Exists(apiPath))
            return apiPath;

        // Último intento: busca desde el directorio raíz del proyecto
        apiPath = Path.Combine(currentDir, "..", "..", "src", "Multitenant.Api");
        if (Directory.Exists(apiPath))
            return apiPath;

        throw new DirectoryNotFoundException($"No se pudo encontrar la carpeta Multitenant.Api. Rutas intentadas desde: {currentDir}");
    }
}

/// <summary>
/// Implementación dummy de ITenantContext para tiempo de diseño.
/// Satisface la dependencia sin requerir contexto HTTP.
/// </summary>
internal class DesignTimeTenantContext : ITenantContext
{
    private Guid? _tenantId = Guid.Empty;

    public Guid? TenantId => _tenantId;

    public void SetTenant(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}