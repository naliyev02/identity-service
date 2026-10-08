using Identity.Application.Features.Authentication.Queries.GetMe;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.Authorization;
using Identity.Domain.User.Enums;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Queries;

public sealed class GetMeHandlerTests
{
    [Fact]
    public async Task MissingUser_ReturnsNotFound()
    {
        //Arrange
        var handler = new GetMeHandler(new FakeUserRepository());

        //Act
        var result = await handler.Handle(new GetMeQuery(Guid.NewGuid()), CancellationToken.None);

        //Assert
        Assert.False(result.Found);
        Assert.Null(result.Id);
        Assert.Empty(result.Roles);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task ExistingUser_ReturnsProfileWithDistinctOrderedAccess()
    {
        //Arrange
        var admin = TestRoles.Admin(TestPermissions.UsersRead(), TestPermissions.ProfileRead());
        var userRole = TestRoles.User(TestPermissions.SessionsRead(), TestPermissions.ProfileRead());
        var user = TestUsers.Active(admin, userRole);
        var handler = new GetMeHandler(new FakeUserRepository(user));

        //Act
        var result = await handler.Handle(new GetMeQuery(user.Id), CancellationToken.None);

        //Assert
        Assert.True(result.Found);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(TestUsers.Email, result.Email);
        Assert.Equal(TestUsers.FirstName, result.FirstName);
        Assert.Equal(TestUsers.LastName, result.LastName);
        Assert.Equal(AccountState.Active, result.State);
        Assert.Equal([RoleNames.Admin, RoleNames.User], result.Roles);
        Assert.Equal(
            [
                PermissionNames.ProfileRead,
                PermissionNames.SessionsRead,
                PermissionNames.UsersRead
            ],
            result.Permissions);
    }
}
