namespace Identity.Application.Features.Authentication.Commands.RevokeMySession;

public sealed record RevokeMySessionResult(bool Found, bool WasCurrent)
{
    public static RevokeMySessionResult Missing() => new(false, false);

    public static RevokeMySessionResult Revoked(bool wasCurrent) => new(true, wasCurrent);
}
