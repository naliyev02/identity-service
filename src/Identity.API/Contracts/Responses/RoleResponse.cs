namespace Identity.API.Contracts.Responses;

public sealed record RoleResponse(Guid Id, string Name, IReadOnlyList<string> Permissions);
