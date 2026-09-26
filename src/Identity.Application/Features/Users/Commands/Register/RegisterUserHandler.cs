using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Domain.User.Entities;
using Identity.Domain.User.ValueObjects;
using MediatR;

namespace Identity.Application.Features.Users.Commands.Register;
public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    public RegisterUserHandler(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            return RegisterUserResult.EmailAlreadyExists();
        var hash = _passwordHasher.Hash(request.PlainPassword);
        var user = new User(
            new Email(request.Email),
            new Password(hash),
            new FullName(request.FirstName, request.LastName));
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RegisterUserResult.Success(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.State);
    }
}
