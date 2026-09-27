namespace Identity.Application.Abstractions.Security;

public interface ITokenHasher
{
    string Hash(string rawToken);
}
