using Identity.Application.Abstractions.Persistence;
using Identity.Application.Services;
using MediatR;

namespace Identity.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    private readonly IUserRepository _users;

    public GetUsersHandler(IUserRepository users) => _users = users;

    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _users.ListAsync(cancellationToken);
        var items = users
            .Select(user => new UserListItem(
                user.Id,
                user.Email.Value,
                user.Name.FirstName,
                user.Name.LastName,
                user.State.ToString(),
                UserAccess.RoleNamesOf(user),
                UserAccess.PermissionsOf(user)))
            .ToArray();

        return new GetUsersResult(items);
    }
}
