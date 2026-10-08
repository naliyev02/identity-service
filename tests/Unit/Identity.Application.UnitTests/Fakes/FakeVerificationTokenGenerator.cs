using Identity.Application.Abstractions.Security;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakeVerificationTokenGenerator : IVerificationTokenGenerator
{
    public string Token { get; set; } = "verify-token";

    public int GenerateCount { get; private set; }

    public string Generate()
    {
        GenerateCount++;
        return Token;
    }
}
