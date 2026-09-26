namespace Identity.Domain.User.Exceptions;
public class InvalidPasswordException : IdentityDomainException
{
    public InvalidPasswordException(string message) : base(message) { }
}
