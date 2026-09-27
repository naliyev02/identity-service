using System.Security.Cryptography;
using Identity.Application.Abstractions.Security;

namespace Identity.Infrastructure.Security;

public sealed class VerificationTokenGenerator : IVerificationTokenGenerator
{
    public string Generate()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> data)
        => Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
