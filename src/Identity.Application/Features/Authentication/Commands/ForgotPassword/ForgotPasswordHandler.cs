using Identity.Application.Abstractions.Persistence;
using Identity.Application.Services;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ForgotPassword;

public sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PasswordResetIssuer _issuer;

    public ForgotPasswordHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        PasswordResetIssuer issuer)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _issuer = issuer;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return;

        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return;

        var rawToken = await _issuer.CreateTokenAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _issuer.SendAsync(user, rawToken, cancellationToken);
    }
}
