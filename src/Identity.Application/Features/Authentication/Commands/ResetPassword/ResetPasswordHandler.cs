using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Domain.User.Exceptions;
using Identity.Domain.User.ValueObjects;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ResetPassword;

public sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IPasswordResetTokenRepository _tokens;
    private readonly IUserRepository _users;
    private readonly ISessionRepository _sessions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordHandler(
        IPasswordResetTokenRepository tokens,
        IUserRepository users,
        ISessionRepository sessions,
        IPasswordHasher passwordHasher,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork)
    {
        _tokens = tokens;
        _users = users;
        _sessions = sessions;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            throw new InvalidPasswordResetTokenException();

        var tokenHash = _tokenHasher.Hash(request.Token.Trim());
        var resetToken = await _tokens.GetByHashAsync(tokenHash, cancellationToken);
        if (resetToken is null)
            throw new InvalidPasswordResetTokenException();

        var utcNow = DateTime.UtcNow;
        if (!resetToken.IsValid(utcNow))
            throw new InvalidPasswordResetTokenException();

        var user = await _users.GetByIdAsync(resetToken.UserId, cancellationToken);
        if (user is null)
            throw new InvalidPasswordResetTokenException();

        var passwordHash = _passwordHasher.Hash(request.NewPassword);
        user.ChangePassword(new Password(passwordHash));
        resetToken.MarkUsed(utcNow);
        await _sessions.RevokeAllForUserAsync(user.Id, utcNow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
