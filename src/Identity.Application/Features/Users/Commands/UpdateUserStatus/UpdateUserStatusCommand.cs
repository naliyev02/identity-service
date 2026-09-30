using MediatR;

namespace Identity.Application.Features.Users.Commands.UpdateUserStatus;

public sealed record UpdateUserStatusCommand(Guid UserId, string Status) : IRequest<UpdateUserStatusResult>;
