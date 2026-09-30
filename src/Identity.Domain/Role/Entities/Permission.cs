using Identity.Domain.Common;

namespace Identity.Domain.Roles.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    protected Permission()
    {
    }

    private Permission(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public static Permission Create(string name, string description)
        => new(name.Trim(), description.Trim());
}
