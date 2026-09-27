namespace Identity.Application.Abstractions.Security;
public interface IPasswordHasher
{
    string Hash(string plainPassword);
    bool Verify(string passwordHash, string plainPassword);
}
