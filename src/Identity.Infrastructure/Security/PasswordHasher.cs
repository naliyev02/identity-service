using Identity.Application.Abstractions.Security;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    public string Hash(string plainPassword)
        => _hasher.HashPassword(new object(), plainPassword);

    public bool Verify(string passwordHash, string plainPassword)
        => _hasher.VerifyHashedPassword(new object(), passwordHash, plainPassword)
           == PasswordVerificationResult.Success;
}
