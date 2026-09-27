namespace Identity.API.Contracts.Requests;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
