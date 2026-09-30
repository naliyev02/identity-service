using Identity.Domain.Authorization;

namespace Identity.Application.Authorization;

public static class AccessPolicies
{
    public const string PermissionClaim = IdentityClaims.Permission;

    public const string ProfileRead = PermissionNames.ProfileRead;
    public const string UsersRead = PermissionNames.UsersRead;
    public const string UsersWrite = PermissionNames.UsersWrite;
    public const string RolesRead = PermissionNames.RolesRead;
    public const string RolesWrite = PermissionNames.RolesWrite;
    public const string SessionsRead = PermissionNames.SessionsRead;
    public const string SessionsRevoke = PermissionNames.SessionsRevoke;

    public static IReadOnlyList<string> All => PermissionNames.All;
}
