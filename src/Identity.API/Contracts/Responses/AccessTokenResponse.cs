namespace Identity.API.Contracts.Responses;

public sealed record AccessTokenResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc);
