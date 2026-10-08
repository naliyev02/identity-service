using Identity.Application.Features.Authentication.Commands.RevokeOtherSessions;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.RevokeOtherSessions;

public sealed class RevokeOtherSessionsHandlerTests
{
    [Fact]
    public async Task MissingCurrentToken_RevokesEveryActiveSession()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var userId = Guid.NewGuid();
        var first = TestSessions.Active(userId, hasher.Hash("first-refresh"));
        var second = TestSessions.Active(userId, hasher.Hash("second-refresh"));
        var foreign = TestSessions.Active(Guid.NewGuid(), hasher.Hash("foreign-refresh"));
        var unitOfWork = new FakeUnitOfWork();
        var sessions = new FakeSessionRepository(first, second, foreign);
        var handler = new RevokeOtherSessionsHandler(sessions, unitOfWork, new CurrentSessionLocator(sessions, hasher));

        //Act
        await handler.Handle(new RevokeOtherSessionsCommand(userId, " "), CancellationToken.None);

        //Assert
        Assert.NotNull(first.RevokedAtUtc);
        Assert.NotNull(second.RevokedAtUtc);
        Assert.Null(foreign.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task ActiveCurrentToken_KeepsThatSession()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var userId = Guid.NewGuid();
        var current = TestSessions.Active(userId, hasher.Hash("current-refresh"));
        var other = TestSessions.Active(userId, hasher.Hash("other-refresh"));
        var unitOfWork = new FakeUnitOfWork();
        var sessions = new FakeSessionRepository(current, other);
        var handler = new RevokeOtherSessionsHandler(sessions, unitOfWork, new CurrentSessionLocator(sessions, hasher));

        //Act
        await handler.Handle(new RevokeOtherSessionsCommand(userId, "current-refresh"), CancellationToken.None);

        //Assert
        Assert.Null(current.RevokedAtUtc);
        Assert.NotNull(other.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task InactiveCurrentToken_RevokesActiveSessions()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var userId = Guid.NewGuid();
        var current = TestSessions.Revoked(userId, hasher.Hash("current-refresh"));
        var other = TestSessions.Active(userId, hasher.Hash("other-refresh"));
        var unitOfWork = new FakeUnitOfWork();
        var sessions = new FakeSessionRepository(current, other);
        var handler = new RevokeOtherSessionsHandler(sessions, unitOfWork, new CurrentSessionLocator(sessions, hasher));

        //Act
        await handler.Handle(new RevokeOtherSessionsCommand(userId, "current-refresh"), CancellationToken.None);

        //Assert
        Assert.NotNull(other.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }
}
