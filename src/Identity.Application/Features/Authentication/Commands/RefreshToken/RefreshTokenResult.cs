namespace Identity.Application.Features.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenResult(
    bool Succeeded,
    string? AccessToken,
    DateTime? AccessTokenExpiresAtUtc,
    string? RefreshToken,
    DateTime? RefreshTokenExpiresAtUtc)
{
    public static RefreshTokenResult Invalid()
        => new(false, null, null, null, null);

    public static RefreshTokenResult Success(
        string accessToken,
        DateTime accessTokenExpiresAtUtc,
        string refreshToken,
        DateTime refreshTokenExpiresAtUtc)
        => new(true, accessToken, accessTokenExpiresAtUtc, refreshToken, refreshTokenExpiresAtUtc);
}
