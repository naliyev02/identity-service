using MediatR;

namespace Identity.Application.Features.Authentication.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest;
