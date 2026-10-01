namespace Identity.API.Contracts.Responses;

public sealed record SessionResponse(Guid Id, DateTime CreatedAtUtc, DateTime ExpiresAtUtc, bool IsCurrent);
