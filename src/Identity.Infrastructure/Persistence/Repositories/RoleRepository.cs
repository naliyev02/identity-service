using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Roles.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
        => await _db.Roles.AddAsync(role, cancellationToken);

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Roles.Include(role => role.Permissions)
            .FirstOrDefaultAsync(role => role.Id == id, cancellationToken);

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        return _db.Roles.Include(role => role.Permissions)
            .FirstOrDefaultAsync(role => role.Name.ToLower() == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken cancellationToken = default)
        => await _db.Roles.Include(role => role.Permissions)
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Role>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
        => await _db.Roles.Include(role => role.Permissions)
            .Where(role => ids.Contains(role.Id))
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLower();
        return _db.Roles.AnyAsync(role => role.Name.ToLower() == normalized, cancellationToken);
    }
}
