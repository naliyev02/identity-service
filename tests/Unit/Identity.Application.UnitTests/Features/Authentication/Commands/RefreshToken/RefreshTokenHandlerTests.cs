using Identity.Application.Features.Authentication.Commands.RefreshToken;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.User.Entities;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenHandlerTests
{
    private const string CurrentToken = "current-refresh";
    private const string NextToken = "next-refresh";

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task BlankToken_ReturnsInvalid(string refreshToken)
    {
        //Arrange
        var handler = Create(new FakeSessionRepository(), new FakeUserRepository()).Handler;

        //Act
        var result = await handler.Handle(new RefreshTokenCommand(refreshToken), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UnknownToken_ReturnsInvalid()
    {
        //Arrange
        var handler = Create(new FakeSessionRepository(), new FakeUserRepository()).Handler;

        //Act
        var result = await handler.Handle(new RefreshTokenCommand(CurrentToken), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExpiredToken_ReturnsInvalid()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var session = TestSessions.Expired(Guid.NewGuid(), hasher.Hash(CurrentToken));
        var handler = Create(new FakeSessionRepository(session), new FakeUserRepository(), hasher).Handler;

        //Act
        var result = await handler.Handle(new RefreshTokenCommand(CurrentToken), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ReusedToken_RevokesAllUserSessions()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var userId = Guid.NewGuid();
        var reused = TestSessions.Active(userId, hasher.Hash(CurrentToken));
        var other = TestSessions.Active(userId, hasher.Hash("other-refresh"));
        var foreign = TestSessions.Active(Guid.NewGuid(), hasher.Hash("foreign-refresh"));
        reused.ReplaceWith(Guid.NewGuid(), DateTime.UtcNow);
        var context = Create(new FakeSessionRepository(reused, other, foreign), new FakeUserRepository(), hasher);

        //Act
        var result = await context.Handler.Handle(new RefreshTokenCommand(CurrentToken), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(other.RevokedAtUtc);
        Assert.Null(foreign.RevokedAtUtc);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task UserWhoCannotSignIn_RevokesSession()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var user = TestUsers.Pending();
        var session = TestSessions.Active(user.Id, hasher.Hash(CurrentToken));
        var context = Create(new FakeSessionRepository(session), new FakeUserRepository(user), hasher);

        //Act
        var result = await context.Handler.Handle(new RefreshTokenCommand(CurrentToken), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(session.RevokedAtUtc);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task ActiveSession_RotatesAndIssuesNewToken()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var user = TestUsers.Active();
        var session = TestSessions.Active(user.Id, hasher.Hash(CurrentToken));
        var context = Create(new FakeSessionRepository(session), new FakeUserRepository(user), hasher);

        //Act
        var result = await context.Handler.Handle(new RefreshTokenCommand(CurrentToken), CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Equal($"access:{user.Id}", result.AccessToken);
        Assert.Equal(NextToken, result.RefreshToken);
        Assert.True(session.WasRotated);
        Assert.NotNull(session.RevokedAtUtc);
        var issued = await context.Sessions.GetByHashAsync(hasher.Hash(NextToken), CancellationToken.None);
        Assert.NotNull(issued);
        Assert.Equal(user.Id, issued.UserId);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    private static RefreshContext Create(
        FakeSessionRepository sessions,
        FakeUserRepository users,
        FakeTokenHasher? hasher = null)
    {
        hasher ??= new FakeTokenHasher();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RefreshTokenHandler(
            sessions,
            users,
            hasher,
            new FakeAccessTokenIssuer(),
            unitOfWork,
            new SessionIssuer(
                sessions,
                new FakeVerificationTokenGenerator { Token = NextToken },
                hasher,
                TestOptions.Jwt()),
            TestOptions.Jwt());

        return new RefreshContext(sessions, unitOfWork, handler);
    }

    private sealed record RefreshContext(
        FakeSessionRepository Sessions,
        FakeUnitOfWork UnitOfWork,
        RefreshTokenHandler Handler);
}
