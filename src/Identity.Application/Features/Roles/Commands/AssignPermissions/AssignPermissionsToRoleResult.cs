namespace Identity.Application.Features.Roles.Commands.AssignPermissions;

public sealed record AssignPermissionsToRoleResult(bool Succeeded, string? ErrorCode)
{
    public static AssignPermissionsToRoleResult Success() => new(true, null);
    public static AssignPermissionsToRoleResult RoleNotFound() => new(false, "RoleNotFound");
    public static AssignPermissionsToRoleResult UnknownPermissions() => new(false, "UnknownPermissions");
}
