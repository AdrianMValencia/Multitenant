using Microsoft.EntityFrameworkCore;
using Multitenant.Application.Abstractions.Multitenancy;
using Multitenant.Application.Abstractions.Security;
using Multitenant.Domain.Entities;
using Multitenant.Infrastructure.Persistence.Context;

namespace Multitenant.Infrastructure.Persistence.Seeding;

/// <summary>
/// Datos demo idempotentes: si ya existen tenant/usuario/cliente, no duplica.
/// GUIDs fijos para poder pegarlos en el login.
/// </summary>
public class DevelopmentDataSeeder(
    ApplicationDbContext context,
    ITenantContext tenantContext,
    ITenantRbacSeeder rbacSeeder,
    IPasswordHasher passwordHasher) : IDevelopmentDataSeeder
{
    public static readonly Guid AcmeTenantId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid NorteTenantId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public const string DemoPassword = "Admin123!"; // Solo Development. Nunca usar en producción.

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedCompanyAsync(
            AcmeTenantId,
            name: "Acme Comercial",
            slug: "acme",
            tenantEmail: "contacto@acme.local",
            adminName: "Admin Acme",
            adminEmail: "admin@acme.local",
            customers:
            [
                ("María López", "maria.lopez@cliente-acme.local", "555-1001", "López & Asociados", "activo"),
                ("Carlos Ruiz", "carlos.ruiz@cliente-acme.local", "555-1002", "Ruiz Importaciones", "activo"),
                ("Ana Torres", "ana.torres@cliente-acme.local", "555-1003", "Torres Retail", "inactivo")
            ],
            cancellationToken);

        await SeedCompanyAsync(
            NorteTenantId,
            name: "Norte Distribuciones",
            slug: "norte",
            tenantEmail: "contacto@norte.local",
            adminName: "Admin Norte",
            adminEmail: "admin@norte.local",
            customers:
            [
                ("Pedro Salinas", "pedro.salinas@cliente-norte.local", "555-2001", "Salinas Logística", "activo"),
                ("Lucía Méndez", "lucia.mendez@cliente-norte.local", "555-2002", "Méndez Alimentos", "activo"),
                ("Jorge Peña", "jorge.pena@cliente-norte.local", "555-2003", "Peña Ferretería", "activo"),
                ("Elena Vargas", "elena.vargas@cliente-norte.local", "555-2004", "Vargas Textiles", "inactivo")
            ],
            cancellationToken);
    }

    private async Task SeedCompanyAsync(
        Guid tenantId,
        string name,
        string slug,
        string tenantEmail,
        string adminName,
        string adminEmail,
        (string Name, string Email, string Phone, string Company, string Status)[] customers,
        CancellationToken cancellationToken)
    {
        tenantContext.SetTenant(tenantId); // El DbContext filtra y asigna TenantId con esto.

        var tenant = await context.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        if (tenant is null)
        {
            context.Tenants.Add(new Tenant
            {
                Id = tenantId,
                Name = name,
                Slug = slug,
                Email = tenantEmail,
                Plan = "demo",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            tenant.Name = name;
            tenant.Slug = slug;
            tenant.Email = tenantEmail;
            tenant.IsActive = true;
            tenant.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        await rbacSeeder.SeedAsync(tenantId, cancellationToken);

        var adminRole = await context.Roles
            .FirstAsync(x => x.TenantId == tenantId && x.Name == "Admin", cancellationToken);

        var user = await context.Users
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == adminEmail, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = adminName,
                Email = adminEmail,
                Password = passwordHasher.Hash(DemoPassword),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
        }

        var hasRole = await context.UserRoles.AnyAsync(
            x => x.TenantId == tenantId && x.UserId == user.Id && x.RoleId == adminRole.Id,
            cancellationToken);

        if (!hasRole)
        {
            context.UserRoles.Add(new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = user.Id,
                RoleId = adminRole.Id
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        var existingCustomerEmails = await context.Customers
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Email)
            .ToListAsync(cancellationToken);

        foreach (var customer in customers)
        {
            if (existingCustomerEmails.Contains(customer.Email))
            {
                continue;
            }

            context.Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Company = customer.Company,
                Status = customer.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
