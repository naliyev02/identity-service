using Identity.Application.Abstractions.Persistence;
using Identity.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _db;

    public PasswordResetTokenRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        => await _db.PasswordResetTokens.AddAsync(token, cancellationToken);

    public Task<PasswordResetToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
        => _db.PasswordResetTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash,
            cancellationToken);

    public async Task InvalidateActiveForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await _db.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Invalidate(utcNow);
    }
}
