using Identity.Domain.User.Entities;

namespace Identity.Application.UnitTests.TestData;

public static class TestSessions
{
    public static Session Active(Guid userId, string tokenHash)
        => Session.Create(userId, tokenHash, TimeSpan.FromDays(7));

    public static Session Expired(Guid userId, string tokenHash)
        => Session.Create(userId, tokenHash, TimeSpan.FromMinutes(-5));

    public static Session Revoked(Guid userId, string tokenHash)
    {
        var session = Active(userId, tokenHash);
        session.Revoke(DateTime.UtcNow);
        return session;
    }
}
