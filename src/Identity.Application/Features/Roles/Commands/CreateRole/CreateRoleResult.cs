namespace Identity.Application.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleResult(bool Succeeded, string? ErrorCode, Guid? Id, string? Name)
{
    public static CreateRoleResult Success(Guid id, string name) => new(true, null, id, name);
    public static CreateRoleResult NameAlreadyExists() => new(false, "RoleAlreadyExists", null, null);
    public static CreateRoleResult InvalidName() => new(false, "InvalidRoleName", null, null);
}
