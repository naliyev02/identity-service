using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Features.Roles.Queries.GetRoleById;

public sealed class GetRoleByIdHandler : IRequestHandler<GetRoleByIdQuery, GetRoleByIdResult>
{
    private readonly IRoleRepository _roles;

    public GetRoleByIdHandler(IRoleRepository roles) => _roles = roles;

    public async Task<GetRoleByIdResult> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
            return GetRoleByIdResult.NotFound();

        var permissions = role.Permissions.Select(permission => permission.Name).OrderBy(name => name).ToArray();
        return GetRoleByIdResult.Success(role.Id, role.Name, permissions);
    }
}
