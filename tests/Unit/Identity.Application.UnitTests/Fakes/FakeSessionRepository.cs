using Identity.Application.Abstractions.Persistence;
using Identity.Domain.User.Entities;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakeSessionRepository : ISessionRepository
{
    private readonly List<Session> _sessions = [];

    public FakeSessionRepository(params Session[] sessions) => _sessions.AddRange(sessions);

    public Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        _sessions.Add(session);
        return Task.CompletedTask;
    }

    public Task<Session?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => Task.FromResult(_sessions.FirstOrDefault(session => session.TokenHash == tokenHash));

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_sessions.FirstOrDefault(session => session.Id == id));

    public Task<IReadOnlyList<Session>> ListActiveByUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Session> sessions = _sessions
            .Where(session => session.UserId == userId && session.IsActive(utcNow))
            .OrderByDescending(session => session.CreatedAt)
            .ToArray();
        return Task.FromResult(sessions);
    }

    public Task RevokeAllForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        foreach (var session in _sessions.Where(session => session.UserId == userId && session.RevokedAtUtc is null))
            session.Revoke(utcNow);

        return Task.CompletedTask;
    }
}
