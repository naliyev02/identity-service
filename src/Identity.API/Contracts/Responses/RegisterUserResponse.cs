namespace Identity.API.Contracts.Responses;

public sealed record RegisterUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string State);
