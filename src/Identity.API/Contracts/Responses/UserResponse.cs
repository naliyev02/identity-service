namespace Identity.API.Contracts.Responses;


public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string State);
