using MediatR;

namespace Identity.Application.Features.Authentication.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : IRequest<VerifyEmailResult>;
