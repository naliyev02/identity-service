using Identity.Application.Abstractions.Persistence;
using Identity.Application.Services;
using MediatR;

namespace Identity.Application.Features.Authentication.Queries.GetMe;

public sealed class GetMeHandler : IRequestHandler<GetMeQuery, GetMeResult>
{
    private readonly IUserRepository _users;

    public GetMeHandler(IUserRepository users) => _users = users;

    public async Task<GetMeResult> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return GetMeResult.NotFound();

        return GetMeResult.Success(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.State,
            UserAccess.RoleNamesOf(user),
            UserAccess.PermissionsOf(user));
    }
}
