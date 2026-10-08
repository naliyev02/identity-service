namespace Identity.Application.Options;

public sealed class LockoutOptions
{
    public const string SectionName = "Lockout";

    public int AttemptLimit { get; set; } = 5;
    public int DurationMinutes { get; set; } = 15;
}
