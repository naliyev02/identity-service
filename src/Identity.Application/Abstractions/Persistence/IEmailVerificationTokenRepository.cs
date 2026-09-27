using Identity.Domain.User.Entities;

namespace Identity.Application.Abstractions.Persistence;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task InvalidateActiveForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default);
}
