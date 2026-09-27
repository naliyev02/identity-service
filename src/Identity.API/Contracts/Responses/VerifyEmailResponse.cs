namespace Identity.API.Contracts.Responses;

public sealed record VerifyEmailResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string State);
