using Identity.Application.Features.Permissions.Queries.GetPermissions;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Permissions.Queries;

public sealed class GetPermissionsHandlerTests
{
    [Fact]
    public async Task EmptyStore_ReturnsNoPermissions()
    {
        //Arrange
        var handler = new GetPermissionsHandler(new FakePermissionRepository());

        //Act
        var result = await handler.Handle(new GetPermissionsQuery(), CancellationToken.None);

        //Assert
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task Permissions_AreMappedByNameAndDescription()
    {
        //Arrange
        var permissions = TestPermissions.All();
        var handler = new GetPermissionsHandler(new FakePermissionRepository(permissions.ToArray()));

        //Act
        var result = await handler.Handle(new GetPermissionsQuery(), CancellationToken.None);

        //Assert
        Assert.Equal(
            permissions
                .OrderBy(permission => permission.Name, StringComparer.Ordinal)
                .Select(permission => (permission.Name, permission.Description)),
            result.Permissions.Select(permission => (permission.Name, permission.Description)));
    }
}
