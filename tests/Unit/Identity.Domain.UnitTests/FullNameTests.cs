using Identity.Domain.User.Exceptions;
using Identity.Domain.User.ValueObjects;
using Xunit;

namespace Identity.Tests.Domain.UnitTests;

public sealed class FullNameTests
{
    [Fact]
    public void Name_IsTrimmed()
    {
        //Act
        var name = new FullName("  Ada  ", "  Lovelace  ");

        //Assert
        Assert.Equal("Ada", name.FirstName);
        Assert.Equal("Lovelace", name.LastName);
        Assert.Equal("Ada Lovelace", name.ToString());
    }

    [Fact]
    public void Name_AcceptsLengthBoundaries()
    {
        //Arrange
        var shortest = new string('a', 2);
        var longest = new string('b', 50);

        //Act
        var name = new FullName(shortest, longest);

        //Assert
        Assert.Equal(shortest, name.FirstName);
        Assert.Equal(longest, name.LastName);
    }

    [Theory]
    [InlineData(null, "Lovelace")]
    [InlineData("", "Lovelace")]
    [InlineData("   ", "Lovelace")]
    [InlineData("A", "Lovelace")]
    [InlineData("Ada", null)]
    [InlineData("Ada", "")]
    [InlineData("Ada", "   ")]
    [InlineData("Ada", "L")]
    public void InvalidName_Throws(string? firstName, string? lastName)
    {
        //Act
        var create = () => new FullName(firstName!, lastName!);

        //Assert
        Assert.Throws<InvalidFullnameException>(create);
    }

    [Fact]
    public void FirstName_Throws_WhenLongerThan50()
    {
        //Act
        var create = () => new FullName(new string('a', 51), "Lovelace");

        //Assert
        Assert.Throws<InvalidFullnameException>(create);
    }

    [Fact]
    public void LastName_Throws_WhenLongerThan50()
    {
        //Act
        var create = () => new FullName("Ada", new string('b', 51));

        //Assert
        Assert.Throws<InvalidFullnameException>(create);
    }
}
