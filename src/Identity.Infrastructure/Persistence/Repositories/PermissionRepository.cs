using Identity.Application.Abstractions.Persistence;
using Identity.Domain.Roles.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _db;

    public PermissionRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Permission>> ListAsync(CancellationToken cancellationToken = default)
        => await _db.Permissions.OrderBy(permission => permission.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Permission>> GetByNamesAsync(
        IReadOnlyCollection<string> names,
        CancellationToken cancellationToken = default)
    {
        var normalized = names.Select(name => name.Trim().ToLower()).ToArray();
        return await _db.Permissions
            .Where(permission => normalized.Contains(permission.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }
}
