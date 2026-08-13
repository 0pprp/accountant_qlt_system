using Domain.Common;
using Domain.Entities.UserAggregate;

namespace Domain.Entities.RoleAggregate;

public class Role : Entity
{
    public Role(string name, string displayName)
    {
        Name = name;
        DisplayName = displayName;
        UserRoles = new HashSet<UserRole>();
        RolePermissions = new HashSet<RolePermission>();
    }

    public string Name { get; set; }
    public string DisplayName { get; set; }

    public ICollection<UserRole>? UserRoles { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}