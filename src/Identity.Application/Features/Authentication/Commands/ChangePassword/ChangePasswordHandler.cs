using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Domain.User.ValueObjects;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ChangePassword;

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly IUserRepository _users;
    private readonly ISessionRepository _sessions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandler(
        IUserRepository users,
        ISessionRepository sessions,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _sessions = sessions;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChangePasswordResult> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return ChangePasswordResult.NotFound();

        if (!_passwordHasher.Verify(user.Password.Hash, request.CurrentPassword))
            return ChangePasswordResult.IncorrectPassword();

        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.ChangePassword(new Password(newHash));

        var utcNow = DateTime.UtcNow;
        await _sessions.RevokeAllForUserAsync(user.Id, utcNow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ChangePasswordResult.Success();
    }
}
