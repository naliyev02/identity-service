using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Roles.Entities;
using MediatR;

namespace Identity.Application.Features.Roles.Commands.CreateRole;

public sealed class CreateRoleHandler : IRequestHandler<CreateRoleCommand, CreateRoleResult>
{
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    {
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateRoleResult> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return CreateRoleResult.InvalidName();

        if (await _roles.ExistsByNameAsync(request.Name, cancellationToken))
            return CreateRoleResult.NameAlreadyExists();

        var role = Role.Create(request.Name);
        await _roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CreateRoleResult.Success(role.Id, role.Name);
    }
}
