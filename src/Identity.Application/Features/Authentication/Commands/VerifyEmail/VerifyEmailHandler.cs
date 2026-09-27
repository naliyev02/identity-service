using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Domain.User.Exceptions;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.VerifyEmail;

public sealed class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    private readonly IEmailVerificationTokenRepository _tokens;
    private readonly IUserRepository _users;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailHandler(
        IEmailVerificationTokenRepository tokens,
        IUserRepository users,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork)
    {
        _tokens = tokens;
        _users = users;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyEmailResult> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new InvalidVerificationTokenException();

        var tokenHash = _tokenHasher.Hash(request.Token.Trim());
        var verificationToken = await _tokens.GetByHashAsync(tokenHash, cancellationToken);
        if (verificationToken is null)
            throw new InvalidVerificationTokenException();

        var utcNow = DateTime.UtcNow;
        if (!verificationToken.IsValid(utcNow))
            throw new InvalidVerificationTokenException();

        var user = await _users.GetByIdAsync(verificationToken.UserId, cancellationToken);
        if (user is null)
            throw new InvalidVerificationTokenException();

        user.VerifyEmail();
        verificationToken.MarkUsed(utcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResult(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.State);
    }
}
