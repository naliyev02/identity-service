namespace Identity.Application.Features.Authentication.Commands.ChangePassword;

public sealed record ChangePasswordResult(bool Succeeded, string? ErrorCode)
{
    public static ChangePasswordResult Success() => new(true, null);
    public static ChangePasswordResult IncorrectPassword() => new(false, "IncorrectPassword");
    public static ChangePasswordResult NotFound() => new(false, "NotFound");
}
