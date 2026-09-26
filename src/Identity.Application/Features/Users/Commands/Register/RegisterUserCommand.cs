using MediatR;

namespace Identity.Application.Features.Users.Commands.Register;
public sealed record RegisterUserCommand(
    string Email,
    string PlainPassword,
    string FirstName,
    string LastName) : IRequest<RegisterUserResult>;
