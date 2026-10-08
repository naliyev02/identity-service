using Identity.Application.Features.Authentication.Queries.GetMySessions;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.User.Entities;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Queries;

public sealed class GetMySessionsHandlerTests
{
    private readonly FakeTokenHasher _hasher = new();

    [Fact]
    public async Task NoActiveSessions_ReturnsEmptyList()
    {
        //Arrange
        var handler = CreateHandler();

        //Act
        var result = await handler.Handle(Query(Guid.NewGuid(), "missing-token"), CancellationToken.None);

        //Assert
        Assert.Empty(result.Sessions);
    }

    [Fact]
    public async Task ActiveSessions_MarkTheMatchingTokenAsCurrent()
    {
        //Arrange
        var userId = Guid.NewGuid();
        const string refreshToken = "current-refresh";
        var current = TestSessions.Active(userId, _hasher.Hash(refreshToken));
        var other = TestSessions.Active(userId, _hasher.Hash("other-refresh"));
        var handler = CreateHandler(current, other);

        //Act
        var result = await handler.Handle(Query(userId, refreshToken), CancellationToken.None);

        //Assert
        Assert.Equal(2, result.Sessions.Count);
        var currentItem = Assert.Single(result.Sessions, session => session.Id == current.Id);
        Assert.True(currentItem.IsCurrent);
        Assert.Equal(current.CreatedAt, currentItem.CreatedAtUtc);
        Assert.Equal(current.ExpiresAtUtc, currentItem.ExpiresAtUtc);
        var otherItem = Assert.Single(result.Sessions, session => session.Id == other.Id);
        Assert.False(otherItem.IsCurrent);
    }

    [Fact]
    public async Task InactiveSessions_AreExcluded()
    {
        //Arrange
        var userId = Guid.NewGuid();
        const string refreshToken = "active-refresh";
        var active = TestSessions.Active(userId, _hasher.Hash(refreshToken));
        var expired = TestSessions.Expired(userId, _hasher.Hash("expired-refresh"));
        var revoked = TestSessions.Revoked(userId, _hasher.Hash("revoked-refresh"));
        var foreign = TestSessions.Active(Guid.NewGuid(), _hasher.Hash("foreign-refresh"));
        var handler = CreateHandler(active, expired, revoked, foreign);

        //Act
        var result = await handler.Handle(Query(userId, refreshToken), CancellationToken.None);

        //Assert
        var session = Assert.Single(result.Sessions);
        Assert.Equal(active.Id, session.Id);
        Assert.True(session.IsCurrent);
    }

    [Fact]
    public async Task AnotherUsersToken_DoesNotMarkCurrent()
    {
        //Arrange
        var userId = Guid.NewGuid();
        const string refreshToken = "foreign-refresh";
        var own = TestSessions.Active(userId, _hasher.Hash("own-refresh"));
        var foreign = TestSessions.Active(Guid.NewGuid(), _hasher.Hash(refreshToken));
        var handler = CreateHandler(own, foreign);

        //Act
        var result = await handler.Handle(Query(userId, refreshToken), CancellationToken.None);

        //Assert
        var session = Assert.Single(result.Sessions);
        Assert.Equal(own.Id, session.Id);
        Assert.False(session.IsCurrent);
    }

    [Fact]
    public async Task RevokedToken_DoesNotMarkADifferentSession()
    {
        //Arrange
        var userId = Guid.NewGuid();
        const string refreshToken = "revoked-refresh";
        var revoked = TestSessions.Revoked(userId, _hasher.Hash(refreshToken));
        var active = TestSessions.Active(userId, _hasher.Hash("active-refresh"));
        var handler = CreateHandler(revoked, active);

        //Act
        var result = await handler.Handle(Query(userId, refreshToken), CancellationToken.None);

        //Assert
        var session = Assert.Single(result.Sessions);
        Assert.Equal(active.Id, session.Id);
        Assert.False(session.IsCurrent);
    }

    [Fact]
    public async Task BlankRefreshToken_MarksNoneAsCurrent()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var active = TestSessions.Active(userId, _hasher.Hash("active-refresh"));
        var handler = CreateHandler(active);

        //Act
        var result = await handler.Handle(Query(userId, "   "), CancellationToken.None);

        //Assert
        var session = Assert.Single(result.Sessions);
        Assert.False(session.IsCurrent);
    }

    private GetMySessionsHandler CreateHandler(params Session[] sessions)
    {
        var repository = new FakeSessionRepository(sessions);
        return new GetMySessionsHandler(repository, new CurrentSessionLocator(repository, _hasher));
    }

    private static GetMySessionsQuery Query(Guid userId, string? refreshToken)
        => new(userId, refreshToken);
}
