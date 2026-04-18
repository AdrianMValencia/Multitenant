using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Multitenant.Application.Multitenancy;
using Multitenant.Domain.Entities;
using Npgsql;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Multitenant.Infrastructure.Persistence.Context;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IConfiguration configuration,
    ITenantContext tenantContext): DbContext(options)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ITenantContext _tenantContext = tenantContext;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        ApplyTenantQueryFilters(modelBuilder); //(WHERE TenantId = @TenantId)
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyTenantRules();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        ApplyTenantRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    public IDbConnection CreateConnection()
        => new NpgsqlConnection(_configuration.GetConnectionString("MultitenantConnection"));

    // Lógica que inyecta el TenantId automáticamente en las entidades antes de guardarlas
    private void ApplyTenantRules()
    {
        var activeTenantId = _tenantContext.TenantId;

        // Buscamos todas las entidades en memoria que implementen ITenantEntity
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            // Para registros nuevos, forzamos el ID de la empresa actual
            if (entry.State == EntityState.Added)
            {
                if (activeTenantId is null)
                {
                    throw new InvalidOperationException("No hay tenant resuelto para la operación actual.");
                }

                entry.Entity.TenantId = activeTenantId.Value;
            }

            // Para modificaciones, validamos que nadie intente cambiar de dueño a un registro (seguridad)
            if (entry.State == EntityState.Modified)
            {
                var originalTenantId = entry.OriginalValues.GetValue<Guid>(nameof(ITenantEntity.TenantId));
                if (entry.Entity.TenantId != originalTenantId)
                {
                    throw new InvalidOperationException("No está permitido cambiar el TenantId de una entidad existente.");
                }
            }
        }
    }

    // Método reflectivo que aplica el filtro WHERE TenantId en todas las entidades multitenant
    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        // Filtramos todas las entidades del modelo que heredan de ITenantEntity
        var tenantEntityTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(t => typeof(ITenantEntity).IsAssignableFrom(t.ClrType));

        foreach (var tenantEntityType in tenantEntityTypes)
        {
            // Usamos reflexión para obtener el método genérico que aplica el filtro real
            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(SetTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)!
                .MakeGenericMethod(tenantEntityType.ClrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    // Inyecta dinámicamente el filtro e.TenantId == _tenantContext.TenantId en cada entidad
    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(BuildTenantPredicate<TEntity>());
    }

    private Expression<Func<TEntity, bool>> BuildTenantPredicate<TEntity>() where TEntity : class, ITenantEntity
    {
        return entity => _tenantContext.TenantId == null || entity.TenantId == _tenantContext.TenantId;
    }
}
