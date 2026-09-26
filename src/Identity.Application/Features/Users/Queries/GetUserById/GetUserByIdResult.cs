using Identity.Domain.User.Enums;

namespace Identity.Application.Features.Users.Queries.GetUserById;
public sealed record GetUserByIdResult(
    bool Found,
    Guid? Id,
    string? Email,
    string? FirstName,
    string? LastName,
    AccountState? State)
{
    public static GetUserByIdResult NotFound() =>
        new(false, null, null, null, null, null);
    public static GetUserByIdResult Success(
        Guid id, string email, string firstName, string lastName, AccountState state) =>
        new(true, id, email, firstName, lastName, state);
}
