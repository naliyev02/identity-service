using MediatR;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public sealed record LogoutCommand(string? RefreshToken) : IRequest;
