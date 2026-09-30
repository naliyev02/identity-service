namespace Identity.Infrastructure.Persistence;

public sealed class IdentitySeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = "admin@identity.local";
    public string AdminPassword { get; set; } = "Admin1!";
}
