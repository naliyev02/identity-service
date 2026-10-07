namespace Identity.API.RateLimiting;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int LoginPermitLimit { get; set; } = 10;
    public int LoginWindowSeconds { get; set; } = 60;
    public int EmailPermitLimit { get; set; } = 5;
    public int EmailWindowSeconds { get; set; } = 300;
}
