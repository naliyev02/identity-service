using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Application.Options;
using Identity.Application.Services;
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

    public LoginHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IAccessTokenIssuer accessTokens,
        IUnitOfWork unitOfWork,
        SessionIssuer sessionIssuer,
        IOptions<JwtOptions> jwtOptions)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _accessTokens = accessTokens;
        _unitOfWork = unitOfWork;
        _sessionIssuer = sessionIssuer;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(user.Password.Hash, request.Password))
            return LoginResult.InvalidCredentials();

        var utcNow = DateTime.UtcNow;
        if (!user.CanSignIn(utcNow))
            return LoginResult.AccountCannotSignIn();

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
