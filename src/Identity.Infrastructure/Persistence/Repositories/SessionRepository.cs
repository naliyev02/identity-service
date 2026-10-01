using Identity.Application.Abstractions.Persistence;
using Identity.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _db;

    public SessionRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
        => await _db.Sessions.AddAsync(session, cancellationToken);

    public Task<Session?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _db.Sessions.FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);

    public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Sessions.FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Session>> ListActiveByUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
        => await _db.Sessions
            .Where(session => session.UserId == userId
                && session.RevokedAtUtc == null
                && session.ExpiresAtUtc > utcNow)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task RevokeAllForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _db.Sessions
            .Where(s => s.UserId == userId && s.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
            session.Revoke(utcNow);
    }
}
