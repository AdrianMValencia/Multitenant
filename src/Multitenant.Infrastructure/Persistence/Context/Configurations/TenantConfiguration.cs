using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Multitenant.Domain.Entities;

namespace Multitenant.Infrastructure.Persistence.Context.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("TenantId");

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.Slug)
            .HasMaxLength(255);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.Plan)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Slug)
            .IsUnique();
    }
}
