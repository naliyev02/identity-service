using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ResendVerification;

public sealed record ResendVerificationCommand(string Email) : IRequest;
