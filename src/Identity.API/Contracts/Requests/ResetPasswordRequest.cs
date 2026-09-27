namespace Identity.API.Contracts.Requests;

public sealed record ResetPasswordRequest(string Token, string NewPassword);
