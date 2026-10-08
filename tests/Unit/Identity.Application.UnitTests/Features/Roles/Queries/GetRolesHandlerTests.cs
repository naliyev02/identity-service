using Identity.Application.Features.Roles.Queries.GetRoles;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.Authorization;
using Xunit;

namespace Identity.Application.UnitTests.Features.Roles.Queries;

public sealed class GetRolesHandlerTests
{
    [Fact]
    public async Task EmptyStore_ReturnsNoRoles()
    {
        //Arrange
        var handler = new GetRolesHandler(new FakeRoleRepository());

        //Act
        var result = await handler.Handle(new GetRolesQuery(), CancellationToken.None);

        //Assert
        Assert.Empty(result.Roles);
    }

    [Fact]
    public async Task Roles_ReturnPermissionsInAlphabeticalOrder()
    {
        //Arrange
        var auditor = TestRoles.Named(
            "Auditor",
            TestPermissions.SessionsRevoke(),
            TestPermissions.ProfileRead());
        var userRole = TestRoles.User(TestPermissions.UsersRead(), TestPermissions.ProfileRead());
        var handler = new GetRolesHandler(new FakeRoleRepository(auditor, userRole));

        //Act
        var result = await handler.Handle(new GetRolesQuery(), CancellationToken.None);

        //Assert
        Assert.Equal(2, result.Roles.Count);
        var auditorItem = Assert.Single(result.Roles, role => role.Id == auditor.Id);
        Assert.Equal("Auditor", auditorItem.Name);
        Assert.Equal(
            [PermissionNames.ProfileRead, PermissionNames.SessionsRevoke],
            auditorItem.Permissions);
        var userItem = Assert.Single(result.Roles, role => role.Id == userRole.Id);
        Assert.Equal(RoleNames.User, userItem.Name);
        Assert.Equal(
            [PermissionNames.ProfileRead, PermissionNames.UsersRead],
            userItem.Permissions);
    }
}
