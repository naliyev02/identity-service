using Identity.Domain.User.Entities;

namespace Identity.Application.Abstractions.Persistence;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task InvalidateActiveForUserAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken = default);
}
