namespace Identity.Application.Features.Authentication.Commands.Login;

public sealed record LoginResult(
    bool Succeeded,
    string? ErrorCode,
    string? AccessToken,
    DateTime? AccessTokenExpiresAtUtc,
    string? RefreshToken,
    DateTime? RefreshTokenExpiresAtUtc)
{
    public static LoginResult InvalidCredentials()
        => new(false, "InvalidCredentials", null, null, null, null);

    public static LoginResult AccountCannotSignIn()
        => new(false, "AccountCannotSignIn", null, null, null, null);

    public static LoginResult Success(
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc)
        => new(true, null, accessToken, accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc);
}
