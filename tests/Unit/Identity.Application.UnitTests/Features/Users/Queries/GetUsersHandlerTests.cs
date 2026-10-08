using Identity.Application.Features.Users.Queries.GetUsers;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.Authorization;
using Xunit;

namespace Identity.Application.UnitTests.Features.Users.Queries;

public sealed class GetUsersHandlerTests
{
    [Fact]
    public async Task EmptyStore_ReturnsNoUsers()
    {
        //Arrange
        var handler = new GetUsersHandler(new FakeUserRepository());

        //Act
        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        //Assert
        Assert.Empty(result.Users);
    }

    [Fact]
    public async Task ActiveUser_IsMappedWithRoleAndPermissions()
    {
        //Arrange
        var user = TestUsers.Active(TestRoles.User());
        var handler = new GetUsersHandler(new FakeUserRepository(user));

        //Act
        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        //Assert
        var item = Assert.Single(result.Users);
        Assert.Equal(user.Id, item.Id);
        Assert.Equal(TestUsers.Email, item.Email);
        Assert.Equal(TestUsers.FirstName, item.FirstName);
        Assert.Equal(TestUsers.LastName, item.LastName);
        Assert.Equal("Active", item.State);
        Assert.Equal([RoleNames.User], item.Roles);
        Assert.Equal(
            [
                PermissionNames.ProfileRead,
                PermissionNames.SessionsRead,
                PermissionNames.SessionsRevoke
            ],
            item.Permissions);
    }

    [Fact]
    public async Task PendingUser_IsMappedWithoutAccess()
    {
        //Arrange
        var user = TestUsers.Pending("grace@identity.local", "Grace", "Hopper");
        var handler = new GetUsersHandler(new FakeUserRepository(user));

        //Act
        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        //Assert
        var item = Assert.Single(result.Users);
        Assert.Equal(user.Id, item.Id);
        Assert.Equal("grace@identity.local", item.Email);
        Assert.Equal("Grace", item.FirstName);
        Assert.Equal("Hopper", item.LastName);
        Assert.Equal("PendingVerification", item.State);
        Assert.Empty(item.Roles);
        Assert.Empty(item.Permissions);
    }
}
