namespace Identity.Domain.User.Exceptions;
public class InvalidFullnameException : IdentityDomainException
{
    public InvalidFullnameException(string message) : base(message)
    {
    }
}
