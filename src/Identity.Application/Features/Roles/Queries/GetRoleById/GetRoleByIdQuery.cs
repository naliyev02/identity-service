using MediatR;

namespace Identity.Application.Features.Roles.Queries.GetRoleById;

public sealed record GetRoleByIdQuery(Guid Id) : IRequest<GetRoleByIdResult>;

public sealed record GetRoleByIdResult(
    bool Found,
    Guid? Id,
    string? Name,
    IReadOnlyList<string> Permissions)
{
    public static GetRoleByIdResult NotFound() => new(false, null, null, []);

    public static GetRoleByIdResult Success(Guid id, string name, IReadOnlyList<string> permissions)
        => new(true, id, name, permissions);
}
