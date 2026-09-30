using MediatR;

namespace Identity.Application.Features.Permissions.Queries.GetPermissions;

public sealed record GetPermissionsQuery : IRequest<GetPermissionsResult>;

public sealed record PermissionListItem(string Name, string Description);

public sealed record GetPermissionsResult(IReadOnlyList<PermissionListItem> Permissions);
