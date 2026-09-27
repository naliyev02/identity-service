using System.Security.Cryptography;
using System.Text;
using Identity.Application.Abstractions.Security;

namespace Identity.Infrastructure.Security;

public sealed class Sha256TokenHasher : ITokenHasher
{
    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
