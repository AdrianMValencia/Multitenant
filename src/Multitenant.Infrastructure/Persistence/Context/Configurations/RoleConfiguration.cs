using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Multitenant.Domain.Entities;

namespace Multitenant.Infrastructure.Persistence.Context.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("RoleId");

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Ignore(r => r.UpdatedAt);

        builder.HasOne(x => x.Tenant)
            .WithMany(t => t.Roles)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
    }
}
