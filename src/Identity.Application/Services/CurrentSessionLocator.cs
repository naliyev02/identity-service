using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Domain.User.Entities;

namespace Identity.Application.Services;

public sealed class CurrentSessionLocator
{
    private readonly ISessionRepository _sessions;
    private readonly ITokenHasher _tokenHasher;

    public CurrentSessionLocator(ISessionRepository sessions, ITokenHasher tokenHasher)
    {
        _sessions = sessions;
        _tokenHasher = tokenHasher;
    }

    public async Task<Session?> FindOwnedAsync(
        Guid userId,
        string? refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var session = await _sessions.GetByHashAsync(_tokenHasher.Hash(refreshToken), cancellationToken);
        if (session is null || session.UserId != userId)
            return null;

        return session;
    }
}
