using MediatR;

namespace Identity.Application.Features.Authentication.Commands.RevokeOtherSessions;

public sealed record RevokeOtherSessionsCommand(Guid UserId, string? RefreshToken) : IRequest;
