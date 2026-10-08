using Identity.Application.Features.Roles.Commands.AssignRolesToUser;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Roles.Commands.AssignRolesToUser;

public sealed class AssignRolesToUserHandlerTests
{
    [Fact]
    public async Task MissingUser_ReturnsUserNotFound()
    {
        //Arrange
        var handler = Handler(new FakeUserRepository(), new FakeRoleRepository());

        //Act
        var result = await handler.Handle(
            new AssignRolesToUserCommand(Guid.NewGuid(), [Guid.NewGuid()]),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("UserNotFound", result.ErrorCode);
    }

    [Fact]
    public async Task UnknownRole_ReturnsUnknownRoles()
    {
        //Arrange
        var user = TestUsers.Active(TestRoles.User());
        var handler = Handler(new FakeUserRepository(user), new FakeRoleRepository(TestRoles.Admin()));

        //Act
        var result = await handler.Handle(
            new AssignRolesToUserCommand(user.Id, [Guid.NewGuid()]),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("UnknownRoles", result.ErrorCode);
    }

    [Fact]
    public async Task KnownRoles_ReplaceUserRoles()
    {
        //Arrange
        var user = TestUsers.Active(TestRoles.User());
        var admin = TestRoles.Admin();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AssignRolesToUserHandler(
            new FakeUserRepository(user),
            new FakeRoleRepository(admin),
            unitOfWork);

        //Act
        var result = await handler.Handle(
            new AssignRolesToUserCommand(user.Id, [admin.Id, admin.Id]),
            CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Equal([admin.Id], user.Roles.Select(role => role.Id));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task EmptyRoles_ClearUserRoles()
    {
        //Arrange
        var user = TestUsers.Active(TestRoles.User());
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AssignRolesToUserHandler(
            new FakeUserRepository(user),
            new FakeRoleRepository(),
            unitOfWork);

        //Act
        var result = await handler.Handle(
            new AssignRolesToUserCommand(user.Id, []),
            CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Empty(user.Roles);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static AssignRolesToUserHandler Handler(FakeUserRepository users, FakeRoleRepository roles)
        => new(users, roles, new FakeUnitOfWork());
}
