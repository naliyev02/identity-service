using Identity.Domain.User.Entities;
using Identity.Domain.User.Exceptions;
using Xunit;

namespace Identity.Tests.Domain.UnitTests;
public sealed class EmailVerificationTokenTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private const string TokenHash = "hashed-verification-token";
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    [Fact]
    public void Create_SetsUserHashAndExpiry()
    {
        //Arrange
        var before = DateTime.UtcNow;

        //Act
        var token = EmailVerificationToken.Create(UserId, TokenHash, Lifetime);

        //Assert
        Assert.Equal(UserId, token.UserId);
        Assert.Equal(TokenHash, token.TokenHash);
        Assert.Null(token.UsedAtUtc);
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.InRange(token.ExpiresAtUtc, before.Add(Lifetime), DateTime.UtcNow.Add(Lifetime));
    }

    [Fact]
    public void Create_Throws_WhenUserIdIsEmpty()
    {
        //Act
        var create = () => EmailVerificationToken.Create(Guid.Empty, TokenHash, Lifetime);

        //Assert
        Assert.Throws<InvalidVerificationTokenException>(create);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Throws_WhenTokenHashIsBlank(string? tokenHash)
    {
        //Act
        var create = () => EmailVerificationToken.Create(UserId, tokenHash!, Lifetime);

        //Assert
        Assert.Throws<InvalidVerificationTokenException>(create);
    }

    [Fact]
    public void IsValid_IsTrue_BeforeExpiry()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, Lifetime);

        //Act
        var isValid = token.IsValid(DateTime.UtcNow);

        //Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_IsTrue_AtExactExpiry()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, Lifetime);
        //Act
        var isValid = token.IsValid(token.ExpiresAtUtc);
        //Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_IsFalse_AfterExpiry()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, TimeSpan.FromMinutes(-1));

        //Act
        var isValid = token.IsValid(DateTime.UtcNow);

        //Assert
        Assert.False(isValid);
    }

    [Fact]
    public void MarkUsed_RecordsUse_WhenTokenIsValid()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, Lifetime);
        var usedAt = DateTime.UtcNow;

        //Act
        token.MarkUsed(usedAt);

        //Assert
        Assert.Equal(usedAt, token.UsedAtUtc);
        Assert.NotNull(token.UpdatedAt);
        Assert.False(token.IsValid(usedAt));
    }

    [Fact]
    public void MarkUsed_Throws_WhenTokenIsExpired()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, TimeSpan.FromMinutes(-1));

        //Act
        var markUsed = () => token.MarkUsed(DateTime.UtcNow);

        //Assert
        Assert.Throws<InvalidVerificationTokenException>(markUsed);
        Assert.Null(token.UsedAtUtc);
    }

    [Fact]
    public void MarkUsed_Throws_WhenTokenWasAlreadyUsed()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, Lifetime);
        var firstUse = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);
        token.MarkUsed(firstUse);

        //Act
        var markUsedAgain = () => token.MarkUsed(firstUse.AddMinutes(1));

        //Assert
        Assert.Throws<InvalidVerificationTokenException>(markUsedAgain);
        Assert.Equal(firstUse, token.UsedAtUtc);
    }


    [Fact]
    public void Invalidate_MarksTokenUsed()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, Lifetime);
        var invalidatedAt = new DateTime(2026, 10, 7, 9, 0, 0, DateTimeKind.Utc);

        //Act
        token.Invalidate(invalidatedAt);

        //Assert
        Assert.Equal(invalidatedAt, token.UsedAtUtc);
        Assert.NotNull(token.UpdatedAt);
        Assert.False(token.IsValid(invalidatedAt));
    }

    [Fact]
    public void MarkUsed_Throws_AfterInvalidate()
    {
        //Arrange
        var token = EmailVerificationToken.Create(UserId, TokenHash, Lifetime);
        var invalidatedAt = new DateTime(2026, 10, 7, 9, 0, 0, DateTimeKind.Utc);
        token.Invalidate(invalidatedAt);

        //Act
        var markUsed = () => token.MarkUsed(invalidatedAt.AddMinutes(1));

        //Assert
        Assert.Throws<InvalidVerificationTokenException>(markUsed);
        Assert.Equal(invalidatedAt, token.UsedAtUtc);
    }

}
