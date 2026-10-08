using Microsoft.Extensions.Options;
using AppOptions = Identity.Application.Options.AppOptions;

namespace Identity.Application.UnitTests.TestData;

public static class TestOptions
{
    public const string PublicBaseUrl = "https://identity.example";

    public static IOptions<AppOptions> App()
        => global::Microsoft.Extensions.Options.Options.Create(new AppOptions { PublicBaseUrl = PublicBaseUrl });
}
