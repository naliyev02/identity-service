namespace Identity.Application.Abstractions.Security;
public interface IPasswordHasher
{
    string Hash(string plainPassword);
}
