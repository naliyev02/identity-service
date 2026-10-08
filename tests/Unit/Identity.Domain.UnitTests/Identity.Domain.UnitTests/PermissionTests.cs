using Identity.Domain.Roles.Entities;
using Xunit;

namespace Identity.Domain.UnitTests;
public sealed class PermissionTests
{
    [Fact]
    public void Create_TrimsNameAndDescription()
    {
        //Act
        var permission = Permission.Create("  users.read  ", "  Read users  ");

        //Assert
        Assert.Equal("users.read", permission.Name);
        Assert.Equal("Read users", permission.Description);
        Assert.NotEqual(Guid.Empty, permission.Id);
        Assert.Null(permission.UpdatedAt);
    }
}
