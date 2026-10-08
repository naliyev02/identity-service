using Identity.Application.Features.Authentication.Commands.RevokeMySession;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.RevokeMySession;

public sealed class RevokeMySessionHandlerTests
{
    private const string RefreshToken = "refresh-token";

    [Fact]
    public async Task MissingSession_ReturnsNotFound()
    {
        //Arrange
        var handler = Create(new FakeSessionRepository(), new FakeTokenHasher());

        //Act
        var result = await handler.Handle(
            new RevokeMySessionCommand(Guid.NewGuid(), Guid.NewGuid(), RefreshToken),
            CancellationToken.None);

        //Assert
        Assert.False(result.Found);
    }

    [Fact]
    public async Task AnotherUsersSession_ReturnsNotFound()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var session = TestSessions.Active(Guid.NewGuid(), hasher.Hash(RefreshToken));
        var handler = Create(new FakeSessionRepository(session), hasher);

        //Act
        var result = await handler.Handle(
            new RevokeMySessionCommand(Guid.NewGuid(), session.Id, RefreshToken),
            CancellationToken.None);

        //Assert
        Assert.False(result.Found);
    }

    [Fact]
    public async Task OwnSession_RevokesIt()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var userId = Guid.NewGuid();
        var session = TestSessions.Active(userId, hasher.Hash("other-refresh"));
        var sessions = new FakeSessionRepository(session);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RevokeMySessionHandler(sessions, unitOfWork, new CurrentSessionLocator(sessions, hasher));

        //Act
        var result = await handler.Handle(
            new RevokeMySessionCommand(userId, session.Id, RefreshToken),
            CancellationToken.None);

        //Assert
        Assert.True(result.Found);
        Assert.False(result.WasCurrent);
        Assert.NotNull(session.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CurrentSession_RevokesItAndMarksCurrent()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var userId = Guid.NewGuid();
        var session = TestSessions.Active(userId, hasher.Hash(RefreshToken));
        var sessions = new FakeSessionRepository(session);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RevokeMySessionHandler(sessions, unitOfWork, new CurrentSessionLocator(sessions, hasher));

        //Act
        var result = await handler.Handle(
            new RevokeMySessionCommand(userId, session.Id, RefreshToken),
            CancellationToken.None);

        //Assert
        Assert.True(result.Found);
        Assert.True(result.WasCurrent);
        Assert.NotNull(session.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static RevokeMySessionHandler Create(FakeSessionRepository sessions, FakeTokenHasher hasher)
        => new(sessions, new FakeUnitOfWork(), new CurrentSessionLocator(sessions, hasher));
}
