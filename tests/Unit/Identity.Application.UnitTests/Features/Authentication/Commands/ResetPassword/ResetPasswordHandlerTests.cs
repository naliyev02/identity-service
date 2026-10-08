using Identity.Application.Features.Authentication.Commands.ResetPassword;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.User.Entities;
using Identity.Domain.User.Exceptions;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.ResetPassword;

public sealed class ResetPasswordHandlerTests
{
    private const string ResetToken = "reset-token";
    private const string NewPassword = "new-password";
    private const string NewPasswordHash = "new-password-hash";

    [Theory]
    [InlineData("", NewPassword)]
    [InlineData(" ", NewPassword)]
    [InlineData(ResetToken, "")]
    [InlineData(ResetToken, " ")]
    public async Task BlankTokenOrPassword_Throws(string token, string newPassword)
    {
        //Arrange
        var handler = Create(new FakePasswordResetTokenRepository(), new FakeUserRepository(), new FakeSessionRepository());

        //Act
        var act = () => handler.Handle(new ResetPasswordCommand(token, newPassword), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(act);
    }

    [Fact]
    public async Task UnknownToken_Throws()
    {
        //Arrange
        var handler = Create(new FakePasswordResetTokenRepository(), new FakeUserRepository(), new FakeSessionRepository());

        //Act
        var act = () => handler.Handle(new ResetPasswordCommand(ResetToken, NewPassword), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(act);
    }

    [Fact]
    public async Task UsedToken_Throws()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var token = PasswordResetToken.Create(Guid.NewGuid(), hasher.Hash(ResetToken), TimeSpan.FromHours(1));
        token.MarkUsed(DateTime.UtcNow);
        var tokens = new FakePasswordResetTokenRepository();
        await tokens.AddAsync(token, CancellationToken.None);
        var handler = Create(tokens, new FakeUserRepository(), new FakeSessionRepository(), hasher);

        //Act
        var act = () => handler.Handle(new ResetPasswordCommand(ResetToken, NewPassword), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(act);
    }

    [Fact]
    public async Task MissingUser_Throws()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var token = PasswordResetToken.Create(Guid.NewGuid(), hasher.Hash(ResetToken), TimeSpan.FromHours(1));
        var tokens = new FakePasswordResetTokenRepository();
        await tokens.AddAsync(token, CancellationToken.None);
        var handler = Create(tokens, new FakeUserRepository(), new FakeSessionRepository(), hasher);

        //Act
        var act = () => handler.Handle(new ResetPasswordCommand(ResetToken, NewPassword), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(act);
    }

    [Fact]
    public async Task ValidToken_ChangesPasswordAndRevokesSessions()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var user = TestUsers.Active();
        var token = PasswordResetToken.Create(user.Id, hasher.Hash(ResetToken), TimeSpan.FromHours(1));
        var tokens = new FakePasswordResetTokenRepository();
        await tokens.AddAsync(token, CancellationToken.None);
        var own = TestSessions.Active(user.Id, "own-session");
        var foreign = TestSessions.Active(Guid.NewGuid(), "foreign-session");
        var passwords = new FakePasswordHasher();
        passwords.Use(NewPassword, NewPasswordHash);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ResetPasswordHandler(
            tokens,
            new FakeUserRepository(user),
            new FakeSessionRepository(own, foreign),
            passwords,
            hasher,
            unitOfWork);

        //Act
        await handler.Handle(new ResetPasswordCommand($"  {ResetToken}  ", NewPassword), CancellationToken.None);

        //Assert
        Assert.Equal(NewPasswordHash, user.Password.Hash);
        Assert.NotNull(token.UsedAtUtc);
        Assert.NotNull(own.RevokedAtUtc);
        Assert.Null(foreign.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static ResetPasswordHandler Create(
        FakePasswordResetTokenRepository tokens,
        FakeUserRepository users,
        FakeSessionRepository sessions,
        FakeTokenHasher? hasher = null)
        => new(tokens, users, sessions, new FakePasswordHasher(), hasher ?? new FakeTokenHasher(), new FakeUnitOfWork());
}
