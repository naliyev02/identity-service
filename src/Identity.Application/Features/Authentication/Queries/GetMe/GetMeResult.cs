using Identity.Domain.User.Enums;

namespace Identity.Application.Features.Authentication.Queries.GetMe;

public sealed record GetMeResult(
    bool Found,
    Guid? Id,
    string? Email,
    string? FirstName,
    string? LastName,
    AccountState? State)
{
    public static GetMeResult NotFound()
        => new(false, null, null, null, null, null);

    public static GetMeResult Success(
        Guid id,
        string email,
        string firstName,
        string lastName,
        AccountState state)
        => new(true, id, email, firstName, lastName, state);
}
