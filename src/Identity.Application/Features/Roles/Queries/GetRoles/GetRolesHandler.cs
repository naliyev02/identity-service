using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Features.Roles.Queries.GetRoles;

public sealed class GetRolesHandler : IRequestHandler<GetRolesQuery, GetRolesResult>
{
    private readonly IRoleRepository _roles;

    public GetRolesHandler(IRoleRepository roles) => _roles = roles;

    public async Task<GetRolesResult> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roles.ListAsync(cancellationToken);
        var items = roles
            .Select(role => new RoleListItem(
                role.Id,
                role.Name,
                role.Permissions.Select(permission => permission.Name).OrderBy(name => name).ToArray()))
            .ToArray();

        return new GetRolesResult(items);
    }
}
