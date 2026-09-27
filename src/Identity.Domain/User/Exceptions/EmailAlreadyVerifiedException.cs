namespace Identity.Domain.User.Exceptions;

public sealed class EmailAlreadyVerifiedException : IdentityDomainException
{
    public EmailAlreadyVerifiedException()
        : base("Email artıq təsdiqlənib.")
    {
    }
}
