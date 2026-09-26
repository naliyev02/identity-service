namespace Identity.Domain.User.Enums;
public enum AccountState
{
    PendingVerification = 1,
    Active = 2,
    LockedOut = 3,
    Suspended = 4,
    Deactivated = 5,
    Archived = 6
}
