using MediatR;

namespace Identity.Application.Features.Users.Queries.GetUserById;
public sealed record GetUserByIdQuery(Guid Id) : IRequest<GetUserByIdResult>;
