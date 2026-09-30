using Identity.Domain.User.Enums;

namespace Identity.Application.Features.Users.Queries.GetUserById;
public sealed record GetUserByIdResult(
    bool Found,
    Guid? Id,
    string? Email,
    string? FirstName,
    string? LastName,
    AccountState? State,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions)
{
    public static GetUserByIdResult NotFound() =>
        new(false, null, null, null, null, null, [], []);

    public static GetUserByIdResult Success(
        Guid id,
        string email,
        string firstName,
        string lastName,
        AccountState state,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions) =>
        new(true, id, email, firstName, lastName, state, roles, permissions);
}
