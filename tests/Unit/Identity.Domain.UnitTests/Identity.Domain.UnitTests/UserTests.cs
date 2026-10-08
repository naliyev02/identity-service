using Identity.Domain.Roles.Entities;
using Identity.Domain.User.Entities;
using Identity.Domain.User.Enums;
using Identity.Domain.User.Exceptions;
using Identity.Domain.User.ValueObjects;
using Xunit;

namespace Identity.Tests.Domain.UnitTests;
public sealed class UserTests
{
    private static User CreateUser()
        => new(
            new Email("test@identity.local"),
            new Password("hashed-password"),
            new FullName("testName", "testSurname"));

    private static User CreateActiveUser()
    {
        var user = CreateUser();
        user.VerifyEmail();
        return user;
    }

    [Fact]
    public void NewAccount_StartsPendingVerification()
    {
        //Arrange
        var user = CreateUser();

        //Assert
        Assert.Equal(AccountState.PendingVerification, user.State);
    }


    [Fact]
    public void PendingAccount_CannotSignIn()
    {
        //Arrange
        var user = CreateUser();

        //Act
        var canSignIn = user.CanSignIn(new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc));

        //Assert
        Assert.False(canSignIn);
    }

    [Fact]
    public void ActiveAccount_CanSignIn()
    {
        //Arrange
        var user = CreateUser();

        //Act
        user.VerifyEmail();
        var canSignIn = user.CanSignIn(new DateTime(2026, 10, 7, 0, 0, 0, DateTimeKind.Utc));

        //Assert
        Assert.True(canSignIn);
    }

    [Fact]
    public void PendingAccount_BecomesActive_WhenEmailIsVerified()
    {
        //Arrange
        var user = CreateUser();

        //Act
        user.VerifyEmail();

        //Assert
        Assert.Equal(AccountState.Active, user.State);
    }

    [Fact]
    public void VerifiedEmail_Throws_WhenVerifiedAgain()
    {
        //Arrange
        var user = CreateUser();

        //Act
        user.VerifyEmail();
        var verifyAgain = () => user.VerifyEmail();

        //Assert
        Assert.Throws<EmailAlreadyVerifiedException>(verifyAgain);
    }

    [Fact]
    public void SuspendedAccount_Throws_WhenEmailIsVerified()
    {
        //Arrange
        var user = CreateUser();

        //Act
        user.ChangeStatus(AccountState.Suspended);
        var verifyEmail = () => user.VerifyEmail();

        //Assert
        Assert.Throws<UserNotActiveException>(verifyEmail);
    }

    [Theory]
    [InlineData(AccountState.PendingVerification)]
    [InlineData(AccountState.LockedOut)]
    [InlineData(AccountState.Archived)]
    public void ChangeStatus_Throws_WhenStateIsNotAllowed(AccountState state)
    {
        //Arrange
        var user = CreateUser();

        //Act
        var changeStatus = () => user.ChangeStatus(state);

        //Assert
        Assert.Throws<InvalidAccountStateException>(changeStatus);
    }

    [Theory]
    [InlineData(AccountState.Active)]
    [InlineData(AccountState.Suspended)]
    [InlineData(AccountState.Deactivated)]
    public void ChangeStatus_SetsAllowedState(AccountState state)
    {
        //Arrange
        var user = CreateUser();

        //Act
        user.ChangeStatus(state);

        //Assert
        Assert.Equal(state, user.State);
    }

    [Fact]
    public void ChangePassword_StoresNewHash_AndClearsLockout()
    {
        //Arrange
        var user = CreateUser();
        var failedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);
        user.RegisterFailedLogin(failedAt, attemptLimit: 1, TimeSpan.FromHours(1));
        var newPassword = new Password("new-hash");

        //Act
        user.ChangePassword(newPassword);

        //Assert
        Assert.Equal(newPassword, user.Password);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEndUtc);
    }

    [Fact]
    public void RegisterFailedLogin_IncrementsAttempts_BeforeTheLimit()
    {
        //Arrange
        var user = CreateUser();
        var failedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);

        //Act
        user.RegisterFailedLogin(failedAt, attemptLimit: 3, TimeSpan.FromMinutes(15));

        //Assert
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEndUtc);
    }

    [Fact]
    public void RegisterFailedLogin_SetsLockout_WhenLimitIsReached()
    {
        //Arrange
        var user = CreateActiveUser();
        var failedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);
        var lockout = TimeSpan.FromMinutes(15);

        //Act
        user.RegisterFailedLogin(failedAt, attemptLimit: 2, lockout);
        user.RegisterFailedLogin(failedAt, attemptLimit: 2, lockout);

        //Assert
        Assert.Equal(2, user.FailedLoginAttempts);
        Assert.Equal(failedAt.Add(lockout), user.LockoutEndUtc);
        Assert.False(user.CanSignIn(failedAt));
    }

    [Fact]
    public void RegisterFailedLogin_LeavesLock_WhileItIsActive()
    {
        //Arrange
        var user = CreateUser();
        var failedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);
        user.RegisterFailedLogin(failedAt, attemptLimit: 1, TimeSpan.FromMinutes(15));

        //Act
        user.RegisterFailedLogin(failedAt.AddMinutes(1), attemptLimit: 1, TimeSpan.FromMinutes(15));

        //Assert
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Equal(failedAt.AddMinutes(15), user.LockoutEndUtc);
    }

    [Fact]
    public void RegisterFailedLogin_StartsAgain_AfterLockoutExpires()
    {
        //Arrange
        var user = CreateActiveUser();
        var failedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);
        user.RegisterFailedLogin(failedAt, attemptLimit: 1, TimeSpan.FromMinutes(15));
        var afterExpiry = failedAt.AddMinutes(15);

        //Act
        user.RegisterFailedLogin(afterExpiry, attemptLimit: 3, TimeSpan.FromMinutes(15));

        //Assert
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEndUtc);
        Assert.True(user.CanSignIn(afterExpiry));
    }

    [Fact]
    public void ResetLockout_ClearsAttemptsAndLockout()
    {
        //Arrange
        var user = CreateUser();
        var failedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);
        user.RegisterFailedLogin(failedAt, attemptLimit: 1, TimeSpan.FromMinutes(15));

        //Act
        user.ResetLockout();

        //Assert
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEndUtc);
    }

    [Fact]
    public void AssignRoles_KeepsEachRoleOnce()
    {
        //Arrange
        var user = CreateUser();

        //Act
        var admin = Role.Create("Admin");
        var member = Role.Create("User");
        user.AssignRoles([admin, member, admin]);

        //Assert
        Assert.Equal([admin, member], user.Roles);
    }

    [Fact]
    public void AssignRoles_ClearsRoles_WhenNoneAreGiven()
    {
        //Arrange
        var user = CreateUser();
        var admin = Role.Create("Admin");
        user.AssignRoles([admin]);

        //Act
        user.AssignRoles([]);

        //Assert
        Assert.Empty(user.Roles);
    }
}