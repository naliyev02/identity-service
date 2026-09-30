using MediatR;

namespace Identity.Application.Features.Roles.Commands.AssignRolesToUser;

public sealed record AssignRolesToUserCommand(Guid UserId, IReadOnlyList<Guid> RoleIds)
    : IRequest<AssignRolesToUserResult>;
