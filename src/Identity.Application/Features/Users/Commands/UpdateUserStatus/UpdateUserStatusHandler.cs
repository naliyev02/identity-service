using Identity.Application.Abstractions.Persistence;
using Identity.Domain.User.Enums;
using MediatR;

namespace Identity.Application.Features.Users.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusHandler : IRequestHandler<UpdateUserStatusCommand, UpdateUserStatusResult>
{
    private static readonly AccountState[] Allowed =
    [
        AccountState.Active,
        AccountState.Suspended,
        AccountState.Deactivated
    ];

    private readonly IUserRepository _users;
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserStatusHandler(
        IUserRepository users,
        ISessionRepository sessions,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateUserStatusResult> Handle(
        UpdateUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AccountState>(request.Status, ignoreCase: true, out var state)
            || !Allowed.Contains(state))
            return UpdateUserStatusResult.InvalidStatus();

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return UpdateUserStatusResult.UserNotFound();

        user.ChangeStatus(state);
        if (state != AccountState.Active)
            await _sessions.RevokeAllForUserAsync(user.Id, DateTime.UtcNow, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UpdateUserStatusResult.Success();
    }
}
