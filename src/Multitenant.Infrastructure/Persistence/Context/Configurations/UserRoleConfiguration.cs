using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Multitenant.Domain.Entities;

namespace Multitenant.Infrastructure.Persistence.Context.Configurations;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.HasOne(x => x.Tenant)
            .WithMany(t => t.UserRoles)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(x => x.RoleId);

        builder.HasIndex(e => new { e.TenantId, e.UserId, e.RoleId }).IsUnique();
    }
}
