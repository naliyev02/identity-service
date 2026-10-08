using Identity.Domain.User.Exceptions;
using Identity.Domain.User.ValueObjects;
using Xunit;

namespace Identity.Tests.Domain.UnitTests;

public sealed class PasswordTests
{
    [Fact]
    public void Hash_IsStored()
    {
        //Act
        var password = new Password("hashed-password");

        //Assert
        Assert.Equal("hashed-password", password.Hash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankHash_Throws(string? hash)
    {
        //Act
        var create = () => new Password(hash!);

        //Assert
        Assert.Throws<InvalidPasswordException>(create);
    }
}
