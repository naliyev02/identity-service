using MediatR;

namespace Identity.Application.Features.Roles.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<GetRolesResult>;

public sealed record RoleListItem(Guid Id, string Name, IReadOnlyList<string> Permissions);

public sealed record GetRolesResult(IReadOnlyList<RoleListItem> Roles);
