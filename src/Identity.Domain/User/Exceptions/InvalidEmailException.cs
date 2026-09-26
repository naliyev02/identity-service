namespace Identity.Domain.User.Exceptions;
public class InvalidEmailException : IdentityDomainException
{
    public InvalidEmailException(string message) : base(message) { }
}
