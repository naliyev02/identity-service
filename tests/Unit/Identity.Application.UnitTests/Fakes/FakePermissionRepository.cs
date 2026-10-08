using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Roles.Entities;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakePermissionRepository : IPermissionRepository
{
    private readonly List<Permission> _permissions = [];

    public FakePermissionRepository(params Permission[] permissions) => _permissions.AddRange(permissions);

    public Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Permission> permissions = _permissions
            .OrderBy(permission => permission.Name, StringComparer.Ordinal)
            .ToArray();
        return Task.FromResult(permissions);
    }

    public Task<IReadOnlyList<Permission>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        var requested = names
            .Select(name => name.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        IReadOnlyList<Permission> permissions = _permissions
            .Where(permission => requested.Contains(permission.Name))
            .ToArray();
        return Task.FromResult(permissions);
    }
}
