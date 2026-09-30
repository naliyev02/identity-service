using Identity.Application.Abstractions.Security;
using Identity.Domain.Authorization;
using Identity.Domain.Roles.Entities;
using Identity.Domain.User.Entities;
using Identity.Domain.User.ValueObjects;
using EmailAddress = Identity.Domain.User.ValueObjects.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    private static readonly (string Name, string Description)[] Catalog =
    [
        (PermissionNames.ProfileRead, "Öz profilini oxumaq"),
        (PermissionNames.UsersRead, "İstifadəçiləri oxumaq"),
        (PermissionNames.UsersWrite, "İstifadəçi rolunu və statusunu dəyişmək"),
        (PermissionNames.RolesRead, "Rol və icazələri oxumaq"),
        (PermissionNames.RolesWrite, "Rol və icazələri dəyişmək"),
        (PermissionNames.SessionsRead, "Sessiyaları oxumaq"),
        (PermissionNames.SessionsRevoke, "Sessiyanı ləğv etmək")
    ];

    public static async Task ApplyAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentitySeedOptions>>().Value;

        await EnsurePermissionsAsync(db, cancellationToken);
        var userRole = await EnsureRoleAsync(db, RoleNames.User, PermissionNames.UserRole, cancellationToken);
        var adminRole = await EnsureRoleAsync(db, RoleNames.Admin, PermissionNames.All, cancellationToken);
        await EnsureAdminAsync(db, hasher, options, adminRole, cancellationToken);
        await AssignDefaultRoleAsync(db, userRole, cancellationToken);
    }

    private static async Task EnsurePermissionsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var existing = await db.Permissions.ToListAsync(cancellationToken);
        foreach (var (name, description) in Catalog)
        {
            if (existing.Any(permission => permission.Name == name))
                continue;

            db.Permissions.Add(Permission.Create(name, description));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Role> EnsureRoleAsync(
        AppDbContext db,
        string roleName,
        IReadOnlyList<string> permissionNames,
        CancellationToken cancellationToken)
    {
        var permissions = await db.Permissions
            .Where(permission => permissionNames.Contains(permission.Name))
            .ToListAsync(cancellationToken);

        var role = await db.Roles
            .Include(item => item.Permissions)
            .FirstOrDefaultAsync(item => item.Name == roleName, cancellationToken);

        if (role is null)
        {
            role = Role.Create(roleName);
            role.ReplacePermissions(permissions);
            db.Roles.Add(role);
        }
        else
        {
            role.ReplacePermissions(permissions);
        }

        await db.SaveChangesAsync(cancellationToken);
        return role;
    }

    private static async Task EnsureAdminAsync(
        AppDbContext db,
        IPasswordHasher hasher,
        IdentitySeedOptions options,
        Role adminRole,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.AdminEmail))
            return;

        var email = options.AdminEmail.Trim().ToLowerInvariant();
        var admin = await db.Users
            .Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.Email.Value.ToLower() == email, cancellationToken);

        if (admin is null)
        {
            if (string.IsNullOrWhiteSpace(options.AdminPassword))
                return;

            admin = new User(
                new EmailAddress(options.AdminEmail.Trim()),
                new Password(hasher.Hash(options.AdminPassword)),
                new FullName("System", "Admin"));
            admin.VerifyEmail();
            admin.AssignRoles([adminRole]);
            db.Users.Add(admin);
        }
        else if (admin.Roles.All(role => role.Id != adminRole.Id))
        {
            admin.AssignRoles(admin.Roles.Append(adminRole));
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task AssignDefaultRoleAsync(
        AppDbContext db,
        Role userRole,
        CancellationToken cancellationToken)
    {
        var unassigned = await db.Users
            .Include(user => user.Roles)
            .Where(user => !user.Roles.Any())
            .ToListAsync(cancellationToken);

        foreach (var user in unassigned)
            user.AssignRoles([userRole]);

        await db.SaveChangesAsync(cancellationToken);
    }
}
