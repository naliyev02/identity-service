using Identity.Application.Abstractions.Persistence;
using Identity.Domain.User.Entities;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];

    public FakeUserRepository(params User[] users) => _users.AddRange(users);

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        => Task.FromResult(_users.Any(user => SameEmail(user, email)));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => Task.FromResult(_users.FirstOrDefault(user => user.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => Task.FromResult(_users.FirstOrDefault(user => SameEmail(user, email)));

    public Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<User> users = _users
            .OrderBy(user => user.Email.Value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return Task.FromResult(users);
    }

    private static bool SameEmail(User user, string email)
        => string.Equals(user.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase);
}
