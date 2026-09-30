using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Features.Roles.Commands.AssignRolesToUser;

public sealed class AssignRolesToUserHandler : IRequestHandler<AssignRolesToUserCommand, AssignRolesToUserResult>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRolesToUserHandler(IUserRepository users, IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _users = users;
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssignRolesToUserResult> Handle(
        AssignRolesToUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return AssignRolesToUserResult.UserNotFound();

        var roleIds = request.RoleIds.Distinct().ToArray();
        var roles = await _roles.GetByIdsAsync(roleIds, cancellationToken);
        if (roles.Count != roleIds.Length)
            return AssignRolesToUserResult.UnknownRoles();

        user.AssignRoles(roles);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return AssignRolesToUserResult.Success();
    }
}
