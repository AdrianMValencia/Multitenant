using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Multitenant.Application.Abstractions.Multitenancy;
using Multitenant.Application.Abstractions.Persistence;
using Multitenant.Application.Abstractions.Security;
using Multitenant.Infrastructure.Multitenancy;
using Multitenant.Infrastructure.Persistence;
using Multitenant.Infrastructure.Persistence.Context;
using Multitenant.Infrastructure.Persistence.Dapper;
using Multitenant.Infrastructure.Persistence.Repositories;
using Multitenant.Infrastructure.Persistence.Seeding;
using Multitenant.Infrastructure.Security;

namespace Multitenant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        // 1. Configuración del DbContext con el motor de PostgreSQL (Npgsql)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("MultitenantConnection")));

        // 2. Mapeo de opciones de configuración para el sistema de seguridad JWT
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // 3. Persistencia y Multitenancy
        services.AddScoped<ITenantContext, TenantContext>(); // El portador del TenantId (Scoped)
        services.AddScoped<IUnitOfWork, UnitOfWork>(); // Gestiona transacciones compartidas
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Repositorio genérico (EF Core)
        services.AddScoped<ITenantDapperExecutor, TenantDapperExecutor>(); // Ejecutor SQL crudo con filtro Tenant (Dapper)

        // 4. Seguridad y RBAC (Control de Acceso Basado en Roles)
        services.AddScoped<IPermissionChecker, PermissionChecker>(); // Verifica permisos granulares
        services.AddScoped<ITenantRbacSeeder, TenantRbacSeeder>(); // Genera permisos iniciales para nuevas empresas
        services.AddSingleton<InMemoryRefreshTokenStore>(); // Almacenamiento seguro de sesiones persistentes
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>(); // Algoritmo robusto de hashing de contraseñas
        services.AddScoped<IAuthService, AuthService>(); // Lógica de expedición de tokens
        return services;
    }
}
