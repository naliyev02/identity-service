using Identity.Domain.User.Enums;

namespace Identity.Application.Features.Users.Commands.Register;
public sealed record RegisterUserResult(
    bool Succeeded,
    string? ErrorCode,
    Guid? Id,
    string? Email,
    string? FirstName,
    string? LastName,
    AccountState? State)
{
    public static RegisterUserResult Success(
        Guid id, string email, string firstName, string lastName, AccountState state) =>
        new(true, null, id, email, firstName, lastName, state);
    public static RegisterUserResult EmailAlreadyExists() =>
        new(false, "EmailAlreadyExists", null, null, null, null, null);
}
