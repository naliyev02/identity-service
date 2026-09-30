using Identity.Domain.Roles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles", "identity");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Name).HasMaxLength(64).IsRequired();
        builder.HasIndex(role => role.Name).IsUnique();

        builder.HasMany(role => role.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "role_permissions",
                permission => permission.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                role => role.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                join =>
                {
                    join.ToTable("role_permissions", "identity");
                    join.HasKey("RoleId", "PermissionId");
                });
    }
}
