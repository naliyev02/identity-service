using Identity.Domain.User.Exceptions;
using Identity.Domain.User.ValueObjects;
using Xunit;

namespace Identity.Tests.Domain.UnitTests;

public sealed class EmailTests
{
    [Fact]
    public void ValidAddress_IsStored()
    {
        //Act
        var email = new Email("test@identity.local");

        //Assert
        Assert.Equal("test@identity.local", email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    public void InvalidAddress_Throws(string? value)
    {
        //Act
        var create = () => new Email(value!);

        //Assert
        Assert.Throws<InvalidEmailException>(create);
    }
}
