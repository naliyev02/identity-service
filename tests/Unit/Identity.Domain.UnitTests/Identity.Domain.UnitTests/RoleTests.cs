using Xunit;
using Identity.Domain.Roles.Entities;

namespace Identity.Domain.UnitTests;
public sealed class RoleTests
{
    [Fact]
    public void Create_TrimsName()
    {
        //Act
        var role = Role.Create("  Admin  ");

        //Assert
        Assert.Equal("Admin", role.Name);
        Assert.NotEqual(Guid.Empty, role.Id);
        Assert.Empty(role.Permissions);
        Assert.Null(role.UpdatedAt);
    }

    [Fact]
    public void ReplacePermissions_SetsTheGivenPermissions()
    {
        //Arrange
        var role = Role.Create("Admin");
        var read = Permission.Create("users.read", "Read users");
        var write = Permission.Create("users.write", "Write users");

        //Act
        role.ReplacePermissions([read, write]);

        //Assert
        Assert.Equal([read, write], role.Permissions);
        Assert.NotNull(role.UpdatedAt);
    }

    [Fact]
    public void ReplacePermissions_DropsPreviousPermissions()
    {
        //Arrange
        var role = Role.Create("Admin");
        var read = Permission.Create("users.read", "Read users");
        var write = Permission.Create("users.write", "Write users");
        role.ReplacePermissions([read, write]);

        //Act
        role.ReplacePermissions([read]);

        //Assert
        Assert.Equal([read], role.Permissions);
    }

    [Fact]
    public void ReplacePermissions_ClearsAll_WhenNoneAreGiven()
    {
        //Arrange
        var role = Role.Create("Admin");
        var read = Permission.Create("users.read", "Read users");
        role.ReplacePermissions([read]);

        //Act
        role.ReplacePermissions([]);

        //Assert
        Assert.Empty(role.Permissions);
    }

    [Fact]
    public void ReplacePermissions_KeepsRepeatedPermissions()
    {
        //Arrange
        var role = Role.Create("Admin");
        var read = Permission.Create("users.read", "Read users");

        //Act
        role.ReplacePermissions([read, read]);

        //Assert
        Assert.Equal([read, read], role.Permissions);
    }
}
