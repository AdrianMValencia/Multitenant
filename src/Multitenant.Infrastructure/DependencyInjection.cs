using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Multitenant.Application.Abstractions.Persistence;
using Multitenant.Application.Multitenancy;
using Multitenant.Infrastructure.Multitenancy;
using Multitenant.Infrastructure.Persistence;
using Multitenant.Infrastructure.Persistence.Context;
using Multitenant.Infrastructure.Persistence.Repositories;

namespace Multitenant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 1. Configuración del DbContext con el motor de PostgreSQL (Npgsql)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("MultitenantConnection")));

        services.AddScoped<ITenantContext, TenantContext>(); // El portador del TenantId (Scoped)
        services.AddScoped<IUnitOfWork, UnitOfWork>(); // Gestiona transacciones compartidas
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Repositorio genérico (EF Core)
        //services.AddScoped<ITenantDapperExecutor, TenantDapperExecutor>(); // Ejecutor SQL crudo con filtro Tenant (Dapper)

        return services;
    }
}
