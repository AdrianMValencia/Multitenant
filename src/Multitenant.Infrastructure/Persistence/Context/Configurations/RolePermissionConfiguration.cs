using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Multitenant.Domain.Entities;

namespace Multitenant.Infrastructure.Persistence.Context.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.HasOne(e => e.Tenant)
            .WithMany(t => t.RolePermissions)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(e => e.RoleId);

        builder.HasOne(e => e.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(e => e.PermissionId);

        builder.HasIndex(e => new { e.TenantId, e.RoleId, e.PermissionId }).IsUnique();
    }
}
