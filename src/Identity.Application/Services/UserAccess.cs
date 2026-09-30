using Identity.Domain.User.Entities;

namespace Identity.Application.Services;

public static class UserAccess
{
    public static string[] RoleNamesOf(User user)
        => user.Roles.Select(role => role.Name).Distinct().OrderBy(name => name).ToArray();

    public static string[] PermissionsOf(User user)
        => user.Roles
            .SelectMany(role => role.Permissions)
            .Select(permission => permission.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToArray();
}
