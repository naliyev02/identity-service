using Identity.Application.Features.Roles.Commands.AssignPermissions;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.Authorization;
using Xunit;

namespace Identity.Application.UnitTests.Features.Roles.Commands.AssignPermissions;

public sealed class AssignPermissionsToRoleHandlerTests
{
    [Fact]
    public async Task MissingRole_ReturnsRoleNotFound()
    {
        //Arrange
        var handler = Handler(new FakeRoleRepository(), new FakePermissionRepository());

        //Act
        var result = await handler.Handle(
            new AssignPermissionsToRoleCommand(Guid.NewGuid(), [PermissionNames.ProfileRead]),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("RoleNotFound", result.ErrorCode);
    }

    [Fact]
    public async Task UnknownPermission_ReturnsUnknownPermissions()
    {
        //Arrange
        var role = TestRoles.Named("Auditor");
        var handler = Handler(
            new FakeRoleRepository(role),
            new FakePermissionRepository(TestPermissions.ProfileRead()));

        //Act
        var result = await handler.Handle(
            new AssignPermissionsToRoleCommand(role.Id, [PermissionNames.ProfileRead, "missing.permission"]),
            CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("UnknownPermissions", result.ErrorCode);
    }

    [Fact]
    public async Task KnownPermissions_ReplaceRolePermissions()
    {
        //Arrange
        var role = TestRoles.Named("Auditor", TestPermissions.UsersRead());
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AssignPermissionsToRoleHandler(
            new FakeRoleRepository(role),
            new FakePermissionRepository(TestPermissions.ProfileRead(), TestPermissions.SessionsRead()),
            unitOfWork);
        var profileName = PermissionNames.ProfileRead;

        //Act
        var result = await handler.Handle(
            new AssignPermissionsToRoleCommand(
                role.Id,
                ["  ", profileName.ToUpperInvariant(), profileName, PermissionNames.SessionsRead]),
            CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Equal(
            [PermissionNames.ProfileRead, PermissionNames.SessionsRead],
            role.Permissions.Select(permission => permission.Name));
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task EmptyPermissions_ClearRolePermissions()
    {
        //Arrange
        var role = TestRoles.Named("Auditor", TestPermissions.ProfileRead());
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AssignPermissionsToRoleHandler(
            new FakeRoleRepository(role),
            new FakePermissionRepository(),
            unitOfWork);

        //Act
        var result = await handler.Handle(
            new AssignPermissionsToRoleCommand(role.Id, []),
            CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Empty(role.Permissions);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static AssignPermissionsToRoleHandler Handler(
        FakeRoleRepository roles,
        FakePermissionRepository permissions)
        => new(roles, permissions, new FakeUnitOfWork());
}
