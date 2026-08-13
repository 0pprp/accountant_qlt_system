using Domain.Common;

namespace Domain.Entities.RoleAggregate;

public class Permission : Entity
{
    public Permission(string name, string displayName, string scope, string scopeDisplayName)
    {
        Name = name;
        DisplayName = displayName;
        Scope = scope;
        ScopeDisplayName = scopeDisplayName;
        RolePermissions = new HashSet<RolePermission>();
        UserPermissions = new HashSet<UserPermission>();
    }

    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Scope { get; set; }
    public string ScopeDisplayName { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; }
    public ICollection<UserPermission> UserPermissions { get; set; }

    public void Update(string displayName, string scope, string scopeDisplayName)
    {
        DisplayName = displayName;
        Scope = scope;
        ScopeDisplayName = scopeDisplayName;
    }
}