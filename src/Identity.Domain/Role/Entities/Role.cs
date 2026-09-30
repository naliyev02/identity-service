using Identity.Domain.Common;

namespace Identity.Domain.Roles.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = null!;
    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    protected Role()
    {
    }

    private Role(string name)
    {
        Name = name.Trim();
    }

    public static Role Create(string name) => new(name);

    public void ReplacePermissions(IEnumerable<Permission> permissions)
    {
        Permissions.Clear();
        foreach (var permission in permissions)
            Permissions.Add(permission);

        SetUpdatedAt();
    }
}
