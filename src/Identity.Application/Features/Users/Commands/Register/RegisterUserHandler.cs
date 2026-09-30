using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using Identity.Application.Services;
using Identity.Domain.Authorization;
using Identity.Domain.User.Entities;
using Identity.Domain.User.ValueObjects;
using MediatR;

namespace Identity.Application.Features.Users.Commands.Register;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly EmailVerificationIssuer _verificationIssuer;

    public RegisterUserHandler(
        IUserRepository users,
        IRoleRepository roles,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        EmailVerificationIssuer verificationIssuer)
    {
        _users = users;
        _roles = roles;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _verificationIssuer = verificationIssuer;
    }

    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            return RegisterUserResult.EmailAlreadyExists();

        var userRole = await _roles.GetByNameAsync(RoleNames.User, cancellationToken)
            ?? throw new InvalidOperationException("User role is not seeded.");

        var hash = _passwordHasher.Hash(request.PlainPassword);
        var user = new User(
            new Email(request.Email),
            new Password(hash),
            new FullName(request.FirstName, request.LastName));
        user.AssignRoles([userRole]);

        await _users.AddAsync(user, cancellationToken);
        var rawToken = await _verificationIssuer.CreateTokenAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _verificationIssuer.SendAsync(user, rawToken, cancellationToken);

        return RegisterUserResult.Success(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.State);
    }
}
