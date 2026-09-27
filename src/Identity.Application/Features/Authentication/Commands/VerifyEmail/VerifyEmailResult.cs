using Identity.Domain.User.Enums;

namespace Identity.Application.Features.Authentication.Commands.VerifyEmail;

public sealed record VerifyEmailResult(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    AccountState State);
