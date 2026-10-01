using Identity.Application.Abstractions.Persistence;
using Identity.Application.Services;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.RevokeMySession;

public sealed class RevokeMySessionHandler : IRequestHandler<RevokeMySessionCommand, RevokeMySessionResult>
{
    private readonly ISessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CurrentSessionLocator _currentSession;

    public RevokeMySessionHandler(
        ISessionRepository sessions,
        IUnitOfWork unitOfWork,
        CurrentSessionLocator currentSession)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _currentSession = currentSession;
    }

    public async Task<RevokeMySessionResult> Handle(
        RevokeMySessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != request.UserId)
            return RevokeMySessionResult.Missing();

        var current = await _currentSession.FindOwnedAsync(
            request.UserId,
            request.RefreshToken,
            cancellationToken);
        session.Revoke(DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RevokeMySessionResult.Revoked(current is not null && session.Id == current.Id);
    }
}
