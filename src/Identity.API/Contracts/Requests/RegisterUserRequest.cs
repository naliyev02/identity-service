namespace Identity.API.Contracts.Requests;

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);
