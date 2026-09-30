using Identity.Domain.User.Enums;

namespace Identity.Application.Features.Authentication.Queries.GetMe;

public sealed record GetMeResult(
    bool Found,
    Guid? Id,
    string? Email,
    string? FirstName,
    string? LastName,
    AccountState? State,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions)
{
    public static GetMeResult NotFound()
        => new(false, null, null, null, null, null, [], []);

    public static GetMeResult Success(
        Guid id,
        string email,
        string firstName,
        string lastName,
        AccountState state,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
        => new(true, id, email, firstName, lastName, state, roles, permissions);
}
