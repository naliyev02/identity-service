using Identity.Domain.User.Entities;
using Xunit;

namespace Identity.Tests.Domain.UnitTests;

public sealed class SessionTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private const string TokenHash = "hashed-refresh-token";
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);
    private static readonly DateTime UtcInstant = new(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_SetsUserHashAndExpiry()
    {
        //Arrange
        var before = DateTime.UtcNow;

        //Act
        var session = Session.Create(UserId, TokenHash, Lifetime);

        //Assert
        Assert.Equal(UserId, session.UserId);
        Assert.Equal(TokenHash, session.TokenHash);
        Assert.Null(session.RevokedAtUtc);
        Assert.Null(session.ReplacedBySessionId);
        Assert.False(session.WasRotated);
        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.InRange(session.ExpiresAtUtc, before.Add(Lifetime), DateTime.UtcNow.Add(Lifetime));
    }

    [Fact]
    public void IsActive_IsTrue_BeforeExpiry()
    {
        //Arrange
        var session = Session.Create(UserId, TokenHash, Lifetime);

        //Act
        var isActive = session.IsActive(DateTime.UtcNow);

        //Assert
        Assert.True(isActive);
    }

    [Fact]
    public void IsActive_IsFalse_AtExactExpiry()
    {
        //Arrange
        var session = Session.Create(UserId, TokenHash, Lifetime);

        //Act
        var isActive = session.IsActive(session.ExpiresAtUtc);

        //Assert
        Assert.False(isActive);
    }

    [Fact]
    public void IsActive_IsFalse_AfterExpiry()
    {
        //Arrange
        var session = Session.Create(UserId, TokenHash, TimeSpan.FromMinutes(-1));

        //Act
        var isActive = session.IsActive(DateTime.UtcNow);

        //Assert
        Assert.False(isActive);
    }

    [Fact]
    public void Revoke_RecordsRevocation()
    {
        //Arrange
        var session = Session.Create(UserId, TokenHash, Lifetime);

        //Act
        session.Revoke(UtcInstant);

        //Assert
        Assert.Equal(UtcInstant, session.RevokedAtUtc);
        Assert.NotNull(session.UpdatedAt);
        Assert.False(session.IsActive(UtcInstant));
        Assert.False(session.WasRotated);
    }

    [Fact]
    public void Revoke_LeavesFirstInstant_WhenAlreadyRevoked()
    {
        //Arrange
        var session = Session.Create(UserId, TokenHash, Lifetime);
        session.Revoke(UtcInstant);

        //Act
        session.Revoke(UtcInstant.AddHours(1));

        //Assert
        Assert.Equal(UtcInstant, session.RevokedAtUtc);
    }

    [Fact]
    public void ReplaceWith_RotatesAndRevokes()
    {
        //Arrange
        var session = Session.Create(UserId, TokenHash, Lifetime);
        var nextSessionId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");

        //Act
        session.ReplaceWith(nextSessionId, UtcInstant);

        //Assert
        Assert.Equal(nextSessionId, session.ReplacedBySessionId);
        Assert.True(session.WasRotated);
        Assert.Equal(UtcInstant, session.RevokedAtUtc);
        Assert.NotNull(session.UpdatedAt);
        Assert.False(session.IsActive(UtcInstant));
    }

    [Fact]
    public void ReplaceWith_KeepsFirstRevocation_WhenAlreadyRevoked()
    {
        //Arrange
        var session = Session.Create(UserId, TokenHash, Lifetime);
        var firstNextId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");
        var secondNextId = Guid.Parse("cccccccc-dddd-eeee-ffff-000000000000");
        session.ReplaceWith(firstNextId, UtcInstant);

        //Act
        session.ReplaceWith(secondNextId, UtcInstant.AddHours(1));

        //Assert
        Assert.Equal(secondNextId, session.ReplacedBySessionId);
        Assert.Equal(UtcInstant, session.RevokedAtUtc);
    }
}