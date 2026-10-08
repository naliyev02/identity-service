using Identity.Application.Features.Roles.Queries.GetRoleById;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.Authorization;
using Xunit;

namespace Identity.Application.UnitTests.Features.Roles.Queries;

public sealed class GetRoleByIdHandlerTests
{
    [Fact]
    public async Task MissingRole_ReturnsNotFound()
    {
        //Arrange
        var handler = new GetRoleByIdHandler(new FakeRoleRepository());

        //Act
        var result = await handler.Handle(new GetRoleByIdQuery(Guid.NewGuid()), CancellationToken.None);

        //Assert
        Assert.False(result.Found);
        Assert.Null(result.Id);
        Assert.Empty(result.Permissions);
    }

    [Fact]
    public async Task ExistingRole_ReturnsPermissionsInAlphabeticalOrder()
    {
        //Arrange
        var role = TestRoles.Named(
            "Auditor",
            TestPermissions.SessionsRevoke(),
            TestPermissions.ProfileRead());
        var handler = new GetRoleByIdHandler(new FakeRoleRepository(role));

        //Act
        var result = await handler.Handle(new GetRoleByIdQuery(role.Id), CancellationToken.None);

        //Assert
        Assert.True(result.Found);
        Assert.Equal(role.Id, result.Id);
        Assert.Equal("Auditor", result.Name);
        Assert.Equal(
            [PermissionNames.ProfileRead, PermissionNames.SessionsRevoke],
            result.Permissions);
    }
}
