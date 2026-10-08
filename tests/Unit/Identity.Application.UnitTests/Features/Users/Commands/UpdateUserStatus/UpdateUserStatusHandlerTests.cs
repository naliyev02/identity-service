using Identity.Application.Features.Users.Commands.UpdateUserStatus;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.User.Entities;
using Identity.Domain.User.Enums;
using Xunit;

namespace Identity.Application.UnitTests.Features.Users.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusHandlerTests
{
    [Theory]
    [InlineData("PendingVerification")]
    [InlineData("LockedOut")]
    [InlineData("Archived")]
    [InlineData(" ")]
    [InlineData("unknown")]
    public async Task DisallowedStatus_ReturnsInvalidStatus(string status)
    {
        //Arrange
        var user = TestUsers.Active();
        var context = CreateContext(user);

        //Act
        var result = await context.Handler.Handle(new UpdateUserStatusCommand(user.Id, status), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("InvalidStatus", result.ErrorCode);
    }

    [Fact]
    public async Task MissingUser_ReturnsUserNotFound()
    {
        //Arrange
        var context = CreateContext();

        //Act
        var result = await context.Handler.Handle(
            new UpdateUserStatusCommand(Guid.NewGuid(), "Suspended"),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("UserNotFound", result.ErrorCode);
    }

    [Fact]
    public async Task Activation_DoesNotRevokeSessions()
    {
        //Arrange
        var user = TestUsers.Pending();
        var session = TestSessions.Active(user.Id, "own-session");
        var context = CreateContext(user, session);

        //Act
        var result = await context.Handler.Handle(
            new UpdateUserStatusCommand(user.Id, "active"),
            CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Equal(AccountState.Active, user.State);
        Assert.Null(session.RevokedAtUtc);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Theory]
    [InlineData("Suspended", AccountState.Suspended)]
    [InlineData("deactivated", AccountState.Deactivated)]
    public async Task BlockingStatus_RevokesOnlyThatUsersSessions(string status, AccountState expected)
    {
        //Arrange
        var user = TestUsers.Active();
        var first = TestSessions.Active(user.Id, "first-session");
        var second = TestSessions.Active(user.Id, "second-session");
        var foreign = TestSessions.Active(Guid.NewGuid(), "foreign-session");
        var context = CreateContext(user, first, second, foreign);

        //Act
        var result = await context.Handler.Handle(new UpdateUserStatusCommand(user.Id, status), CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Equal(expected, user.State);
        Assert.NotNull(first.RevokedAtUtc);
        Assert.NotNull(second.RevokedAtUtc);
        Assert.Null(foreign.RevokedAtUtc);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    private static StatusContext CreateContext(User? user = null, params Session[] sessions)
    {
        var users = user is null ? new FakeUserRepository() : new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UpdateUserStatusHandler(users, new FakeSessionRepository(sessions), unitOfWork);
        return new StatusContext(unitOfWork, handler);
    }

    private sealed class StatusContext(FakeUnitOfWork unitOfWork, UpdateUserStatusHandler handler)
    {
        public FakeUnitOfWork UnitOfWork { get; } = unitOfWork;
        public UpdateUserStatusHandler Handler { get; } = handler;
    }
}
