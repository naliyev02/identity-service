using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Roles.Entities;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakeRoleRepository : IRoleRepository
{
    private readonly List<Role> _roles = [];

    public FakeRoleRepository(params Role[] roles) => _roles.AddRange(roles);

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        _roles.Add(role);
        return Task.CompletedTask;
    }

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_roles.FirstOrDefault(role => role.Id == id));

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(_roles.FirstOrDefault(role => SameName(role, name)));

    public Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Role> roles = _roles
            .OrderBy(role => role.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return Task.FromResult(roles);
    }

    public Task<IReadOnlyList<Role>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var requested = ids.ToHashSet();
        IReadOnlyList<Role> roles = _roles.Where(role => requested.Contains(role.Id)).ToArray();
        return Task.FromResult(roles);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult(_roles.Any(role => SameName(role, name)));

    private static bool SameName(Role role, string name)
        => string.Equals(role.Name, name.Trim(), StringComparison.OrdinalIgnoreCase);
}
