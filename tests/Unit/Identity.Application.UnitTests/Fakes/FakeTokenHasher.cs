using Identity.Application.Abstractions.Security;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakeTokenHasher : ITokenHasher
{
    public string Hash(string rawToken) => $"hash:{rawToken}";
}
