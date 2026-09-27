using MediatR;

namespace Identity.Application.Features.Authentication.Queries.GetMe;

public sealed record GetMeQuery(Guid UserId) : IRequest<GetMeResult>;
