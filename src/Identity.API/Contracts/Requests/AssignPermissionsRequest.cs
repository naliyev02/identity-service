namespace Identity.API.Contracts.Requests;

public sealed record AssignPermissionsRequest(IReadOnlyList<string> Permissions);
