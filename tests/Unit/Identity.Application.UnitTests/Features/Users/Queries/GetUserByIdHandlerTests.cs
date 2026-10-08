using Identity.Application.Features.Users.Queries.GetUserById;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.Authorization;
using Identity.Domain.User.Enums;
using Xunit;

namespace Identity.Application.UnitTests.Features.Users.Queries;

public sealed class GetUserByIdHandlerTests
{
    [Fact]
    public async Task MissingUser_ReturnsNotFound()
    {
        //Arrange
        var handler = new GetUserByIdHandler(new FakeUserRepository());

        //Act
        var result = await handler.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        //Assert
        Assert.False(result.Found);
        Assert.Null(result.Id);
        Assert.Empty(result.Roles);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task ExistingUser_ReturnsProfileWithRolesAndPermissions()
    {
        //Arrange
        var role = TestRoles.User();
        var user = TestUsers.Active(role);
        var handler = new GetUserByIdHandler(new FakeUserRepository(user));

        //Act
        var result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        //Assert
        Assert.True(result.Found);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(TestUsers.Email, result.Email);
        Assert.Equal(TestUsers.FirstName, result.FirstName);
        Assert.Equal(TestUsers.LastName, result.LastName);
        Assert.Equal(AccountState.Active, result.State);
        Assert.Equal([RoleNames.User], result.Roles);
        Assert.Equal(
            [
                PermissionNames.ProfileRead,
                PermissionNames.SessionsRead,
                PermissionNames.SessionsRevoke
            ],
            result.Permissions);
    }
}
