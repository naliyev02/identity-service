using MediatR;

namespace Identity.Application.Features.Authentication.Queries.GetMySessions;

public sealed record GetMySessionsQuery(Guid UserId, string? RefreshToken) : IRequest<GetMySessionsResult>;

public sealed record SessionItem(Guid Id, DateTime CreatedAtUtc, DateTime ExpiresAtUtc, bool IsCurrent);

public sealed record GetMySessionsResult(IReadOnlyList<SessionItem> Sessions);
