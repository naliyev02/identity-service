using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Application.Options;
using Identity.Application.Services;
using Identity.Domain.User.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace Identity.Application.Features.Authentication.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenIssuer _accessTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SessionIssuer _sessionIssuer;
    private readonly JwtOptions _jwtOptions;
    private readonly LockoutOptions _lockout;

    public LoginHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IAccessTokenIssuer accessTokens,
        IUnitOfWork unitOfWork,
        SessionIssuer sessionIssuer,
        IOptions<JwtOptions> jwtOptions,
        IOptions<LockoutOptions> lockout)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _accessTokens = accessTokens;
        _unitOfWork = unitOfWork;
        _sessionIssuer = sessionIssuer;
        _jwtOptions = jwtOptions.Value;
        _lockout = lockout.Value;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return LoginResult.InvalidCredentials();

        var utcNow = DateTime.UtcNow;
        if (!_passwordHasher.Verify(user.Password.Hash, request.Password))
            return await RejectFailedPassword(user, utcNow, cancellationToken);

        if (!user.CanSignIn(utcNow))
            return LoginResult.AccountCannotSignIn();

        user.ResetLockout();
        return await IssueLogin(user, utcNow, cancellationToken);
    }

    private async Task<LoginResult> RejectFailedPassword(
        User user,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        user.RegisterFailedLogin(
            utcNow,
            _lockout.AttemptLimit,
            TimeSpan.FromMinutes(_lockout.DurationMinutes));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return LoginResult.InvalidCredentials();
    }

    private async Task<LoginResult> IssueLogin(
        User user,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var session = await _sessionIssuer.IssueAsync(user, cancellationToken);
        var accessTokenExpiresAtUtc = utcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var accessToken = _accessTokens.Issue(user, accessTokenExpiresAtUtc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LoginResult.Success(
            accessToken,
            accessTokenExpiresAtUtc,
            session.RefreshToken,
            session.ExpiresAtUtc);
    }
}
