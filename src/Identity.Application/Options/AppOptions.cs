namespace Identity.Application.Options;

public sealed class AppOptions
{
    public const string SectionName = "App";

    public string PublicBaseUrl { get; set; } = "http://local.identity.com";
}
