namespace Identity.Domain.User.Exceptions;

public sealed class InvalidPasswordResetTokenException : IdentityDomainException
{
    public InvalidPasswordResetTokenException()
        : base("Şifrə sıfırlama tokeni etibarsızdır və ya müddəti bitib.")
    {
    }
}
