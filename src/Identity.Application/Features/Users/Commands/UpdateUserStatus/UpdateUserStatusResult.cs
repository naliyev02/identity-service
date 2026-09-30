namespace Identity.Application.Features.Users.Commands.UpdateUserStatus;

public sealed record UpdateUserStatusResult(bool Succeeded, string? ErrorCode)
{
    public static UpdateUserStatusResult Success() => new(true, null);
    public static UpdateUserStatusResult UserNotFound() => new(false, "UserNotFound");
    public static UpdateUserStatusResult InvalidStatus() => new(false, "InvalidStatus");
}
