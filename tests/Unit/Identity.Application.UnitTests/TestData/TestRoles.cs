using Identity.Domain.Authorization;
using Identity.Domain.Roles.Entities;

namespace Identity.Application.UnitTests.TestData;

public static class TestRoles
{
    public static Role User(params Permission[] permissions)
        => Create(RoleNames.User, permissions.Length == 0 ? TestPermissions.ForUserRole() : permissions);

    public static Role Admin(params Permission[] permissions)
        => Create(RoleNames.Admin, permissions.Length == 0 ? TestPermissions.All() : permissions);

    public static Role Named(string name, params Permission[] permissions)
        => Create(name, permissions);

    private static Role Create(string name, IEnumerable<Permission> permissions)
    {
        var role = Role.Create(name);
        var assigned = permissions.ToArray();
        if (assigned.Length > 0)
            role.ReplacePermissions(assigned);

        return role;
    }
}
