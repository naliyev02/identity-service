namespace Identity.Application.Features.Roles.Commands.AssignRolesToUser;

public sealed record AssignRolesToUserResult(bool Succeeded, string? ErrorCode)
{
    public static AssignRolesToUserResult Success() => new(true, null);
    public static AssignRolesToUserResult UserNotFound() => new(false, "UserNotFound");
    public static AssignRolesToUserResult UnknownRoles() => new(false, "UnknownRoles");
}
