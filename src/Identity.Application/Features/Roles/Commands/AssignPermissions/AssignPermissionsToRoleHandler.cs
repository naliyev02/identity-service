using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Features.Roles.Commands.AssignPermissions;

public sealed class AssignPermissionsToRoleHandler
    : IRequestHandler<AssignPermissionsToRoleCommand, AssignPermissionsToRoleResult>
{
    private readonly IRoleRepository _roles;
    private readonly IPermissionRepository _permissions;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionsToRoleHandler(
        IRoleRepository roles,
        IPermissionRepository permissions,
        IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _permissions = permissions;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssignPermissionsToRoleResult> Handle(
        AssignPermissionsToRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            return AssignPermissionsToRoleResult.RoleNotFound();

        var requested = request.Permissions
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = await _permissions.GetByNamesAsync(requested, cancellationToken);
        if (permissions.Count != requested.Length)
            return AssignPermissionsToRoleResult.UnknownPermissions();

        role.ReplacePermissions(permissions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return AssignPermissionsToRoleResult.Success();
    }
}
