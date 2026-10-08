using Identity.Application.Abstractions.Persistence;
using Identity.Domain.User.Entities;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakePasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly List<PasswordResetToken> _tokens = [];

    public IReadOnlyList<PasswordResetToken> Tokens => _tokens;

    public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        _tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokens.FirstOrDefault(token => token.TokenHash == tokenHash));

    public Task InvalidateActiveForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        foreach (var token in _tokens.Where(token => token.UserId == userId && token.UsedAtUtc is null))
            token.Invalidate(utcNow);

        return Task.CompletedTask;
    }
}
