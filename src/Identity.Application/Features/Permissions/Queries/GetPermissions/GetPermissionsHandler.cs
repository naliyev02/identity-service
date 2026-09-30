using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Features.Permissions.Queries.GetPermissions;

public sealed class GetPermissionsHandler : IRequestHandler<GetPermissionsQuery, GetPermissionsResult>
{
    private readonly IPermissionRepository _permissions;

    public GetPermissionsHandler(IPermissionRepository permissions) => _permissions = permissions;

    public async Task<GetPermissionsResult> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _permissions.ListAsync(cancellationToken);
        var items = permissions
            .Select(permission => new PermissionListItem(permission.Name, permission.Description))
            .ToArray();

        return new GetPermissionsResult(items);
    }
}
