using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.Login;

public sealed class LoginHandlerTests
{
    private const string PlainPassword = "plain-password";
    private const string RefreshToken = "refresh-token";

    [Fact]
    public async Task UnknownEmail_ReturnsInvalidCredentials()
    {
        //Arrange
        var context = Create();

        //Act
        var result = await context.Handler.Handle(
            new LoginCommand(TestUsers.Email, PlainPassword),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("InvalidCredentials", result.ErrorCode);
    }

    [Fact]
    public async Task WrongPassword_RecordsFailedAttempt()
    {
        //Arrange
        var user = TestUsers.Active();
        var context = Create(user);
        var unitOfWork = context.UnitOfWork;

        //Act
        var result = await context.Handler.Handle(
            new LoginCommand(TestUsers.Email, "wrong-password"),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("InvalidCredentials", result.ErrorCode);
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UserWhoCannotSignIn_ReturnsAccountCannotSignIn()
    {
        //Arrange
        var user = TestUsers.Pending();
        var context = Create(user);
        context.Passwords.Use(PlainPassword, TestUsers.PasswordHash);

        //Act
        var result = await context.Handler.Handle(
            new LoginCommand(TestUsers.Email, PlainPassword),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("AccountCannotSignIn", result.ErrorCode);
    }

    [Fact]
    public async Task ActiveUser_IssuesAccessTokenAndSession()
    {
        //Arrange
        var user = TestUsers.Active();
        var context = Create(user);
        context.Passwords.Use(PlainPassword, TestUsers.PasswordHash);

        //Act
        var result = await context.Handler.Handle(
            new LoginCommand(TestUsers.Email, PlainPassword),
            CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Equal($"access:{user.Id}", result.AccessToken);
        Assert.Equal(RefreshToken, result.RefreshToken);
        var session = await context.Sessions.GetByHashAsync(
            new FakeTokenHasher().Hash(RefreshToken),
            CancellationToken.None);
        Assert.NotNull(session);
        Assert.Equal(user.Id, session.UserId);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    private static LoginContext Create(params Identity.Domain.User.Entities.User[] users)
    {
        var userRepository = new FakeUserRepository(users);
        var sessions = new FakeSessionRepository();
        var passwords = new FakePasswordHasher();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new LoginHandler(
            userRepository,
            passwords,
            new FakeAccessTokenIssuer(),
            unitOfWork,
            new SessionIssuer(
                sessions,
                new FakeVerificationTokenGenerator { Token = RefreshToken },
                new FakeTokenHasher(),
                TestOptions.Jwt()),
            TestOptions.Jwt(),
            TestOptions.Lockout());

        return new LoginContext(sessions, passwords, unitOfWork, handler);
    }

    private sealed record LoginContext(
        FakeSessionRepository Sessions,
        FakePasswordHasher Passwords,
        FakeUnitOfWork UnitOfWork,
        LoginHandler Handler);
}
