using Identity.Domain.User.Entities;

namespace Identity.Application.Abstractions.Persistence;

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default);
}
