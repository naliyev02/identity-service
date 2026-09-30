using MediatR;

namespace Identity.Application.Features.Roles.Commands.CreateRole;

public sealed record CreateRoleCommand(string Name) : IRequest<CreateRoleResult>;
