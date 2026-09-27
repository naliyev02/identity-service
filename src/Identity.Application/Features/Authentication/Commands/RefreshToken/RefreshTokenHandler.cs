using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Application.Options;
using Identity.Application.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Authentication.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly ISessionRepository _sessions;
    private readonly IUserRepository _users;
    private readonly ITokenHasher _tokenHasher;
    private readonly IAccessTokenIssuer _accessTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionIssuer _sessionIssuer;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenHandler(
        ISessionRepository sessions,
        IUserRepository users,
        ITokenHasher tokenHasher,
        IAccessTokenIssuer accessTokens,
        IUnitOfWork unitOfWork,
        SessionIssuer sessionIssuer,
        IOptions<JwtOptions> jwtOptions)
    {
        _sessions = sessions;
        _users = users;
        _tokenHasher = tokenHasher;
        _accessTokens = accessTokens;
        _unitOfWork = unitOfWork;
        _sessionIssuer = sessionIssuer;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<RefreshTokenResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return RefreshTokenResult.Invalid();

        var session = await _sessions.GetByHashAsync(
            _tokenHasher.Hash(request.RefreshToken),
            cancellationToken);
        if (session is null)
            return RefreshTokenResult.Invalid();

        var utcNow = DateTime.UtcNow;
        if (!session.IsActive(utcNow))
        {
            if (session.WasRotated)
            {
                await _sessions.RevokeAllForUserAsync(session.UserId, utcNow, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return RefreshTokenResult.Invalid();
        }

        var user = await _users.GetByIdAsync(session.UserId, cancellationToken);
        if (user is null || !user.CanSignIn(utcNow))
        {
            session.Revoke(utcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return RefreshTokenResult.Invalid();
        }

        var issued = await _sessionIssuer.IssueAsync(user, cancellationToken);
        session.ReplaceWith(issued.SessionId, utcNow);

        var accessTokenExpiresAtUtc = utcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var accessToken = _accessTokens.Issue(user, accessTokenExpiresAtUtc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RefreshTokenResult.Success(
            accessToken,
            accessTokenExpiresAtUtc,
            issued.RefreshToken,
            issued.ExpiresAtUtc);
    }
}
