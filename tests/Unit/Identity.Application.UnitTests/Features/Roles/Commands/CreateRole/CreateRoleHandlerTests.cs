using Identity.Application.Features.Roles.Commands.CreateRole;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Roles.Commands.CreateRole;

public sealed class CreateRoleHandlerTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task BlankName_ReturnsInvalidRoleName(string name)
    {
        //Arrange
        var roles = new FakeRoleRepository();
        var handler = new CreateRoleHandler(roles, new FakeUnitOfWork());

        //Act
        var result = await handler.Handle(new CreateRoleCommand(name), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("InvalidRoleName", result.ErrorCode);
    }

    [Fact]
    public async Task ExistingName_ReturnsRoleAlreadyExists()
    {
        //Arrange
        var roles = new FakeRoleRepository(TestRoles.Admin());
        var handler = new CreateRoleHandler(roles, new FakeUnitOfWork());

        //Act
        var result = await handler.Handle(new CreateRoleCommand("  ADMIN  "), CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("RoleAlreadyExists", result.ErrorCode);
    }

    [Fact]
    public async Task NewName_StoresTrimmedRole()
    {
        //Arrange
        var roles = new FakeRoleRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateRoleHandler(roles, unitOfWork);

        //Act
        var result = await handler.Handle(new CreateRoleCommand("  Auditor  "), CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        var role = Assert.Single(await roles.ListAsync(CancellationToken.None));
        Assert.Equal(role.Id, result.Id);
        Assert.Equal("Auditor", role.Name);
        Assert.Equal(1, unitOfWork.SaveCount);
    }
}
