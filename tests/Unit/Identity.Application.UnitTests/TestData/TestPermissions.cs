using Identity.Domain.Authorization;
using Identity.Domain.Roles.Entities;

namespace Identity.Application.UnitTests.TestData;

public static class TestPermissions
{
    public static Permission ProfileRead()
        => Permission.Create(PermissionNames.ProfileRead, "Öz profilini oxumaq");

    public static Permission UsersRead()
        => Permission.Create(PermissionNames.UsersRead, "İstifadəçiləri oxumaq");

    public static Permission UsersWrite()
        => Permission.Create(PermissionNames.UsersWrite, "İstifadəçi rolunu və statusunu dəyişmək");

    public static Permission RolesRead()
        => Permission.Create(PermissionNames.RolesRead, "Rol və icazələri oxumaq");

    public static Permission RolesWrite()
        => Permission.Create(PermissionNames.RolesWrite, "Rol və icazələri dəyişmək");

    public static Permission SessionsRead()
        => Permission.Create(PermissionNames.SessionsRead, "Sessiyaları oxumaq");

    public static Permission SessionsRevoke()
        => Permission.Create(PermissionNames.SessionsRevoke, "Sessiyanı ləğv etmək");

    public static IReadOnlyList<Permission> All()
        =>
        [
            ProfileRead(),
            UsersRead(),
            UsersWrite(),
            RolesRead(),
            RolesWrite(),
            SessionsRead(),
            SessionsRevoke()
        ];

    public static IReadOnlyList<Permission> ForUserRole()
        =>
        [
            ProfileRead(),
            SessionsRead(),
            SessionsRevoke()
        ];
}
