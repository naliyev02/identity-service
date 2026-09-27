using MediatR;

namespace Identity.Application.Features.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;
