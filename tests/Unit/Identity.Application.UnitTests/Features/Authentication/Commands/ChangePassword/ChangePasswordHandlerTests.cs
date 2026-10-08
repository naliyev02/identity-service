using Identity.Application.Features.Authentication.Commands.ChangePassword;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.ChangePassword;

public sealed class ChangePasswordHandlerTests
{
    private const string CurrentPassword = "plain-password";
    private const string NewPassword = "new-password";
    private const string NewPasswordHash = "new-password-hash";

    [Fact]
    public async Task MissingUser_ReturnsNotFound()
    {
        //Arrange
        var handler = new ChangePasswordHandler(
            new FakeUserRepository(),
            new FakeSessionRepository(),
            new FakePasswordHasher(),
            new FakeUnitOfWork());

        //Act
        var result = await handler.Handle(
            new ChangePasswordCommand(Guid.NewGuid(), CurrentPassword, NewPassword),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    [Fact]
    public async Task WrongCurrentPassword_ReturnsIncorrectPassword()
    {
        //Arrange
        var user = TestUsers.Active();
        var handler = new ChangePasswordHandler(
            new FakeUserRepository(user),
            new FakeSessionRepository(),
            new FakePasswordHasher(),
            new FakeUnitOfWork());

        //Act
        var result = await handler.Handle(
            new ChangePasswordCommand(user.Id, "wrong-password", NewPassword),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("IncorrectPassword", result.ErrorCode);
    }

    [Fact]
    public async Task CorrectPassword_ChangesHashAndRevokesSessions()
    {
        //Arrange
        var user = TestUsers.Active();
        var own = TestSessions.Active(user.Id, "own-session");
        var foreign = TestSessions.Active(Guid.NewGuid(), "foreign-session");
        var passwords = new FakePasswordHasher();
        passwords.Use(CurrentPassword, TestUsers.PasswordHash);
        passwords.Use(NewPassword, NewPasswordHash);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ChangePasswordHandler(
            new FakeUserRepository(user),
            new FakeSessionRepository(own, foreign),
            passwords,
            unitOfWork);

        //Act
        var result = await handler.Handle(
            new ChangePasswordCommand(user.Id, CurrentPassword, NewPassword),
            CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Equal(NewPasswordHash, user.Password.Hash);
        Assert.NotNull(own.RevokedAtUtc);
        Assert.Null(foreign.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }
}
