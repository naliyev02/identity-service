using Identity.Application.Abstractions.Persistence;
using MediatR;

namespace Identity.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdResult>
{
    private readonly IUserRepository _users;
    public GetUserByIdHandler(IUserRepository users) => _users = users;
    public async Task<GetUserByIdResult> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return GetUserByIdResult.NotFound();
        return GetUserByIdResult.Success(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.State);
    }
}
