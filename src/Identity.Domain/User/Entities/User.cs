using Identity.Domain.Common;
using Identity.Domain.Roles.Entities;
using Identity.Domain.User.Enums;
using Identity.Domain.User.Exceptions;
using Identity.Domain.User.ValueObjects;

namespace Identity.Domain.User.Entities;

public class User : BaseEntity, IAggregateRoot
{
    public Email Email { get; private set; } = null!;
    public Password Password { get; private set; } = null!;
    public FullName Name { get; private set; } = null!;
    public AccountState State { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public string? SuspensionReason { get; private set; }
    public ICollection<Role> Roles { get; private set; } = new List<Role>();

    protected User()
    {
    }

    public User(Email email, Password password, FullName name)
    {
        Email = email;
        Password = password;
        Name = name;
        State = AccountState.PendingVerification;
        FailedLoginAttempts = 0;
    }

    public void VerifyEmail()
    {
        if (State == AccountState.Active)
            throw new EmailAlreadyVerifiedException();

        if (State != AccountState.PendingVerification)
            throw new UserNotActiveException();

        State = AccountState.Active;
        SetUpdatedAt();
    }

    public bool CanSignIn(DateTime utcNow)
    {
        if (State != AccountState.Active)
            return false;

        if (LockoutEndUtc is not null && LockoutEndUtc > utcNow)
            return false;

        return true;
    }

    public void ChangePassword(Password password)
    {
        Password = password;
        FailedLoginAttempts = 0;
        LockoutEndUtc = null;
        SetUpdatedAt();
    }

    public void AssignRoles(IEnumerable<Role> roles)
    {
        Roles.Clear();
        foreach (var role in roles.DistinctBy(role => role.Id))
            Roles.Add(role);

        SetUpdatedAt();
    }

    public void ChangeStatus(AccountState state)
    {
        if (state is not (AccountState.Active or AccountState.Suspended or AccountState.Deactivated))
            throw new InvalidAccountStateException();

        State = state;
        SetUpdatedAt();
    }
}
