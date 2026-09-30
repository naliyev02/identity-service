using MediatR;

namespace Identity.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<GetUsersResult>;

public sealed record UserListItem(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string State,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record GetUsersResult(IReadOnlyList<UserListItem> Users);
