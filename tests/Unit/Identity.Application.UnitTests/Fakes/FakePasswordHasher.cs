using Identity.Application.Abstractions.Security;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakePasswordHasher : IPasswordHasher
{
    private readonly Dictionary<string, string> _hashes = [];

    public void Use(string plainPassword, string hash) => _hashes[plainPassword] = hash;

    public string Hash(string plainPassword) => HashOf(plainPassword);

    public bool Verify(string passwordHash, string plainPassword)
        => _hashes.TryGetValue(plainPassword, out var hash) && passwordHash == hash;

    private string HashOf(string plainPassword)
        => _hashes.TryGetValue(plainPassword, out var hash)
            ? hash
            : throw new InvalidOperationException("Plain password has no hash.");
}
