using Identity.Domain.Roles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions", "identity");
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Name).HasMaxLength(128).IsRequired();
        builder.Property(permission => permission.Description).HasMaxLength(256).IsRequired();
        builder.HasIndex(permission => permission.Name).IsUnique();
    }
}
