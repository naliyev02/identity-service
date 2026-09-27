using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Application.Options;
using Identity.Domain.User.Entities;
using Microsoft.Extensions.Options;

namespace Identity.Application.Services;

public sealed record IssuedSession(Guid SessionId, string RefreshToken, DateTime ExpiresAtUtc);

public sealed class SessionIssuer
{
    private readonly ISessionRepository _sessions;
    private readonly IVerificationTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly JwtOptions _jwtOptions;

    public SessionIssuer(
        ISessionRepository sessions,
        IVerificationTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IOptions<JwtOptions> jwtOptions)
    {
        _sessions = sessions;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<IssuedSession> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        var refreshToken = _tokenGenerator.Generate();
        var session = Session.Create(
            user.Id,
            _tokenHasher.Hash(refreshToken),
            TimeSpan.FromDays(_jwtOptions.RefreshTokenDays));

        await _sessions.AddAsync(session, cancellationToken);
        return new IssuedSession(session.Id, refreshToken, session.ExpiresAtUtc);
    }
}
