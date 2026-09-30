using MediatR;

namespace Identity.Application.Features.Roles.Commands.AssignPermissions;

public sealed record AssignPermissionsToRoleCommand(Guid RoleId, IReadOnlyList<string> Permissions)
    : IRequest<AssignPermissionsToRoleResult>;
