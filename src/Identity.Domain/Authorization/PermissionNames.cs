namespace Identity.Domain.Authorization;

public static class PermissionNames
{
    public const string ProfileRead = "identity.profile.read";
    public const string UsersRead = "identity.users.read";
    public const string UsersWrite = "identity.users.write";
    public const string RolesRead = "identity.roles.read";
    public const string RolesWrite = "identity.roles.write";
    public const string SessionsRead = "identity.sessions.read";
    public const string SessionsRevoke = "identity.sessions.revoke";

    public static readonly IReadOnlyList<string> All =
    [
        ProfileRead,
        UsersRead,
        UsersWrite,
        RolesRead,
        RolesWrite,
        SessionsRead,
        SessionsRevoke
    ];

    public static readonly IReadOnlyList<string> UserRole = [ProfileRead];
}
