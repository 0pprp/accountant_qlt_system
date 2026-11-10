using Domain.Entities.RoleAggregate;

namespace Application.Users.Common;

/// <summary>
/// (صلاحيات الأدوار ∪ السماحات الخاصة) − المنع الخاص.
/// المقارنة بمعرف الصلاحية أو اسمها الثابت — وليس مرجع الكائن.
/// </summary>
public static class EffectivePermissionCalculator
{
    public static List<Permission> Compute(
        IEnumerable<Permission> rolePermissions,
        IEnumerable<Permission> allowPermissions,
        IEnumerable<Permission> denyPermissions)
    {
        var deniedIds = denyPermissions
            .Select(p => p.Id)
            .Where(id => id > 0)
            .ToHashSet();

        var deniedNames = denyPermissions
            .Select(p => p.Name)
            .Where(n => string.IsNullOrWhiteSpace(n) == false)
            .ToHashSet(StringComparer.Ordinal);

        var byKey = new Dictionary<string, Permission>(StringComparer.Ordinal);

        foreach (var permission in rolePermissions.Concat(allowPermissions))
        {
            if (permission is null || string.IsNullOrWhiteSpace(permission.Name))
                continue;

            if (deniedIds.Contains(permission.Id) || deniedNames.Contains(permission.Name))
                continue;

            byKey[permission.Name] = permission;
        }

        return byKey.Values.ToList();
    }

    public static List<Permission> ComputeByIds(
        IEnumerable<int> rolePermissionIds,
        IEnumerable<int> allowPermissionIds,
        IEnumerable<int> denyPermissionIds,
        IReadOnlyDictionary<int, Permission> permissionsById)
    {
        var denied = denyPermissionIds.ToHashSet();
        var resultIds = rolePermissionIds
            .Concat(allowPermissionIds)
            .Where(id => denied.Contains(id) == false)
            .Distinct();

        return resultIds
            .Where(permissionsById.ContainsKey)
            .Select(id => permissionsById[id])
            .ToList();
    }
}
