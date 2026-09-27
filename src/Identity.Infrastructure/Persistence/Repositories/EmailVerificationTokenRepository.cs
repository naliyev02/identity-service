using Identity.Application.Abstractions.Persistence;
using Identity.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AppDbContext _db;

    public EmailVerificationTokenRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
        => await _db.EmailVerificationTokens.AddAsync(token, cancellationToken);

    public Task<EmailVerificationToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
        => _db.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash,
            cancellationToken);

    public async Task InvalidateActiveForUserAsync(
        Guid userId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await _db.EmailVerificationTokens
            .Where(t => t.UserId == userId && t.UsedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Invalidate(utcNow);
    }
}
