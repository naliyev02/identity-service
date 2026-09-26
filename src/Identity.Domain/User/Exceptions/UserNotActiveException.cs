namespace Identity.Domain.User.Exceptions;
public class UserNotActiveException : IdentityDomainException
{
    public UserNotActiveException()
        : base("İstifadəçi hesabı aktiv deyil. Əməliyyat dayandırıldı.") { }
}
