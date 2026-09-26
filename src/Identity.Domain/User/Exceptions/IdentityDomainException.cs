namespace Identity.Domain.User.Exceptions;
public abstract class IdentityDomainException : Exception
{
    protected IdentityDomainException(string message) : base(message) { }
}
