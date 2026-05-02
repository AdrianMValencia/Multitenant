using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Multitenant.Domain.Entities;

namespace Multitenant.Infrastructure.Persistence.Context.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(e => e.Name).HasMaxLength(255);
        builder.Property(e => e.Resource).HasMaxLength(255);
        builder.Property(e => e.Action).HasMaxLength(255);

        builder.HasOne(e => e.Tenant)
            .WithMany(t => t.Permissions)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.Resource, e.Action }).IsUnique();
    }
}
