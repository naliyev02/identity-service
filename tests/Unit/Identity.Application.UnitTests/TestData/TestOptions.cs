using Microsoft.Extensions.Options;
using AppOptions = Identity.Application.Options.AppOptions;
using JwtOptions = Identity.Application.Options.JwtOptions;
using LockoutOptions = Identity.Application.Options.LockoutOptions;

namespace Identity.Application.UnitTests.TestData;

public static class TestOptions
{
    public const string PublicBaseUrl = "https://identity.example";

    public static IOptions<AppOptions> App()
        => global::Microsoft.Extensions.Options.Options.Create(new AppOptions { PublicBaseUrl = PublicBaseUrl });

    public static IOptions<JwtOptions> Jwt()
        => global::Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

    public static IOptions<LockoutOptions> Lockout()
        => global::Microsoft.Extensions.Options.Options.Create(new LockoutOptions
        {
            AttemptLimit = 5,
            DurationMinutes = 15
        });
}
