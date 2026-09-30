namespace Identity.Domain.User.Exceptions;

public sealed class InvalidAccountStateException : IdentityDomainException
{
    public InvalidAccountStateException()
        : base("Bu hesab vəziyyəti təyin edilə bilməz.")
    {
    }
}
