using Identity.Domain.User.Exceptions;

namespace Identity.Domain.User.ValueObjects;
public record Password
{
    public string Hash { get; }

    public Password(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new InvalidPasswordException("Parol hash-i boş ola bilməz.");

        Hash = hash;
    }
}
