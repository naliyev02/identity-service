using MediatR;

namespace Identity.Application.Features.Authentication.Commands.RevokeMySession;

public sealed record RevokeMySessionCommand(Guid UserId, Guid SessionId, string? RefreshToken)
    : IRequest<RevokeMySessionResult>;
