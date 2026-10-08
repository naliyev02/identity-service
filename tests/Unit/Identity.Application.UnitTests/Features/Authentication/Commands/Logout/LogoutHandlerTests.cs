using Identity.Application.Features.Authentication.Commands.Logout;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.Logout;

public sealed class LogoutHandlerTests
{
    [Fact]
    public async Task BlankToken_LeavesSessionActive()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var session = TestSessions.Active(Guid.NewGuid(), hasher.Hash("refresh-token"));
        var handler = new LogoutHandler(new FakeSessionRepository(session), hasher, new FakeUnitOfWork());

        //Act
        await handler.Handle(new LogoutCommand(" "), CancellationToken.None);

        //Assert
        Assert.Null(session.RevokedAtUtc);
    }

    [Fact]
    public async Task UnknownToken_LeavesSessionActive()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var session = TestSessions.Active(Guid.NewGuid(), hasher.Hash("refresh-token"));
        var handler = new LogoutHandler(new FakeSessionRepository(session), hasher, new FakeUnitOfWork());

        //Act
        await handler.Handle(new LogoutCommand("other-token"), CancellationToken.None);

        //Assert
        Assert.Null(session.RevokedAtUtc);
    }

    [Fact]
    public async Task KnownToken_RevokesSession()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var session = TestSessions.Active(Guid.NewGuid(), hasher.Hash("refresh-token"));
        var unitOfWork = new FakeUnitOfWork();
        var handler = new LogoutHandler(new FakeSessionRepository(session), hasher, unitOfWork);

        //Act
        await handler.Handle(new LogoutCommand("refresh-token"), CancellationToken.None);

        //Assert
        Assert.NotNull(session.RevokedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }
}
