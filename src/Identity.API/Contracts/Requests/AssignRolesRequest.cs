namespace Identity.API.Contracts.Requests;

public sealed record AssignRolesRequest(IReadOnlyList<Guid> RoleIds);
