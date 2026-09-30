using Identity.Domain.Roles.Entities;

namespace Identity.Application.Abstractions.Persistence;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Permission>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default);
}
