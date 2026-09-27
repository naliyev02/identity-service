namespace Identity.Domain.User.Exceptions;

public sealed class InvalidVerificationTokenException : IdentityDomainException
{
    public InvalidVerificationTokenException()
        : base("Təsdiq tokeni etibarsızdır və ya müddəti bitib.")
    {
    }
}
