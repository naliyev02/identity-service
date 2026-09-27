using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest;
