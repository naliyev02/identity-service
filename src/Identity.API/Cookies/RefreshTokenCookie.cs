namespace Identity.API.Cookies;

public static class RefreshTokenCookie
{
    public const string Name = "refresh_token";
    public const string Path = "/api/auth";

    public static CookieOptions Create(bool isDevelopment, DateTimeOffset? expires = null)
        => new()
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Lax,
            Path = Path,
            Expires = expires
        };
}
