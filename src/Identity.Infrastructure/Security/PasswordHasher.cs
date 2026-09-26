using Identity.Application.Abstractions.Security;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    public string Hash(string plainPassword)
        => _hasher.HashPassword(new object(), plainPassword);
}
