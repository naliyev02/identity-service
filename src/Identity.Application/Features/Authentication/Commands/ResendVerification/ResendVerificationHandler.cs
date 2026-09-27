using Identity.Application.Abstractions.Persistence;
using Identity.Application.Services;
using Identity.Domain.User.Enums;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ResendVerification;

public sealed class ResendVerificationHandler : IRequestHandler<ResendVerificationCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmailVerificationIssuer _issuer;

    public ResendVerificationHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        EmailVerificationIssuer issuer)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _issuer = issuer;
    }

    public async Task Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return;

        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return;

        if (user.State != AccountState.PendingVerification)
            return;

        var rawToken = await _issuer.CreateTokenAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _issuer.SendAsync(user, rawToken, cancellationToken);
    }
}
