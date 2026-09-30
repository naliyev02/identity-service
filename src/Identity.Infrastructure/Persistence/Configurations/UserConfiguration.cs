using Identity.Domain.Roles.Entities;
using Identity.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users","identity");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.State).HasConversion<int>().IsRequired();
        builder.Property(u => u.FailedLoginAttempts).IsRequired();
        builder.Property(u => u.LockoutEndUtc);
        builder.Property(u => u.SuspensionReason);
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();
            email.HasIndex(e => e.Value).IsUnique();
        });
        builder.OwnsOne(u => u.Password, password =>
        {
            password.Property(p => p.Hash)
                .HasColumnName("PasswordHash")
                .IsRequired();
        });
        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(50)
                .IsRequired();
            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.HasMany(user => user.Roles)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "user_roles",
                role => role.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                user => user.HasOne<User>().WithMany().HasForeignKey("UserId"),
                join =>
                {
                    join.ToTable("user_roles", "identity");
                    join.HasKey("UserId", "RoleId");
                });
    }
}
