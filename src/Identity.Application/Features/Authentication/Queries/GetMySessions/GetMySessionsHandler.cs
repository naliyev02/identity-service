using Identity.Application.Abstractions.Persistence;
using Identity.Application.Services;
using MediatR;

namespace Identity.Application.Features.Authentication.Queries.GetMySessions;

public sealed class GetMySessionsHandler : IRequestHandler<GetMySessionsQuery, GetMySessionsResult>
{
    private readonly ISessionRepository _sessions;
    private readonly CurrentSessionLocator _currentSession;

    public GetMySessionsHandler(ISessionRepository sessions, CurrentSessionLocator currentSession)
    {
        _sessions = sessions;
        _currentSession = currentSession;
    }

    public async Task<GetMySessionsResult> Handle(
        GetMySessionsQuery request,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var sessions = await _sessions.ListActiveByUserAsync(request.UserId, utcNow, cancellationToken);
        var current = await _currentSession.FindOwnedAsync(request.UserId, request.RefreshToken, cancellationToken);

        var items = sessions
            .Select(session => new SessionItem(
                session.Id,
                session.CreatedAt,
                session.ExpiresAtUtc,
                current is not null && session.Id == current.Id))
            .ToArray();

        return new GetMySessionsResult(items);
    }
}
