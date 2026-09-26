using Identity.Domain.Common;
using Identity.Domain.User.Enums;
using Identity.Domain.User.ValueObjects;
using System.Xml.Linq;

namespace Identity.Domain.User.Entities;
public class User : BaseEntity, IAggregateRoot
{
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public FullName Name { get; private set; }
    public AccountState State { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public string? SuspensionReason { get; private set; }

    protected User() { }

    public User(Email email, Password password, FullName name)
    {
        // Id və CreatedAt BaseEntity-dən avtomatik gəlir
        Email = email;
        Password = password;
        Name = name;
        State = AccountState.PendingVerification; // Yeni hesab həmişə təsdiq gözləyir
        FailedLoginAttempts = 0;
    }
}
