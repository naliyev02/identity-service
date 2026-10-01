using Identity.Application.Abstractions.Persistence;
using Identity.Application.Services;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.RevokeOtherSessions;

public sealed class RevokeOtherSessionsHandler : IRequestHandler<RevokeOtherSessionsCommand>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentSessionLocator _currentSession;

    public RevokeOtherSessionsHandler(
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        CurrentSessionLocator currentSession)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _currentSession = currentSession;
    }

    public async Task Handle(RevokeOtherSessionsCommand request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var current = await _currentSession.FindOwnedAsync(request.UserId, request.RefreshToken, cancellationToken);
        var keepId = current is not null && current.IsActive(utcNow) ? current.Id : (Guid?)null;
        var sessions = await _sessions.ListActiveByUserAsync(request.UserId, utcNow, cancellationToken);

        foreach (var session in sessions)
        {
            if (session.Id == keepId)
                continue;

            session.Revoke(utcNow);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
