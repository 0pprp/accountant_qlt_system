using Application.Common.AdminPanel;
using Application.Common.Interfaces;
using Domain.Entities.RoleAggregate;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Users.Common;

public sealed class PanelTabMigrationReviewRow
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public string TabKey { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty; // open | closed | review
    public string? OwnedActions { get; init; }
    public string? WouldGainActions { get; init; }
    public string? Reason { get; init; }
}

public sealed class PanelTabMigrationResult
{
    public bool Applied { get; init; }
    public int UsersScanned { get; init; }
    public int OverridesWritten { get; init; }
    public required List<PanelTabMigrationReviewRow> ReviewRows { get; init; }
    public string SnapshotPath { get; init; } = string.Empty;
}

/// <summary>
/// تحويل حسابات قديمة إلى استثناءات Panel.Tab.* على قاعدة معزولة.
/// كامل الحزمة → مفتوح؛ صفر → مقفول؛ جزئي → مقفول + مراجعة.
/// </summary>
public static class PanelTabAccountMigrator
{
    public static async Task<PanelTabMigrationResult> RunAsync(
        IApplicationDbContext db,
        bool apply,
        string? snapshotDirectory,
        CancellationToken cancellationToken = default)
    {
        var users = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role!).ThenInclude(r => r.RolePermissions)
            .Include(u => u.UserPermissions)
            .Include(u => u.UserPermissionDenials)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var allPermissions = await db.Permissions.ToListAsync(cancellationToken);
        var byId = allPermissions.ToDictionary(p => p.Id);
        var byName = allPermissions
            .Where(p => string.IsNullOrWhiteSpace(p.Name) == false)
            .GroupBy(p => p.Name, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var snapshot = new List<object>();
        var review = new List<PanelTabMigrationReviewRow>();
        var overridesWritten = 0;

        foreach (var user in users)
        {
            var roleName = user.UserRoles.Select(ur => ur.Role!.Name).FirstOrDefault() ?? "";
            var rolePerms = user.UserRoles
                .SelectMany(ur => ur.Role!.RolePermissions)
                .Select(rp => byId.GetValueOrDefault(rp.PermissionId))
                .Where(p => p is not null)
                .Cast<Permission>()
                .ToList();
            var allows = user.UserPermissions
                .Select(up => byId.GetValueOrDefault(up.PermissionId))
                .Where(p => p is not null)
                .Cast<Permission>()
                .ToList();
            var denials = user.UserPermissionDenials
                .Select(d => byId.GetValueOrDefault(d.PermissionId))
                .Where(p => p is not null)
                .Cast<Permission>()
                .ToList();

            snapshot.Add(new
            {
                user.Id,
                user.Username,
                roleName,
                user.PermissionsVersion,
                Allows = allows.Select(p => p.Name).ToList(),
                Denials = denials.Select(p => p.Name).ToList(),
                Role = rolePerms.Select(p => p.Name).ToList()
            });

            // حسابات Admin محمية — لا نكتب استثناءات عليها
            if (roleName == SeedRoles.Admin.Name)
                continue;

            var rawEffective = EffectivePermissionCalculator.ComputeRaw(rolePerms, allows, denials);
            var rawNames = rawEffective.Select(p => p.Name).ToHashSet(StringComparer.Ordinal);

            foreach (var tab in AdminPanelTabs.ReadyTabs)
            {
                var required = tab.GrantedActions.ToHashSet(StringComparer.Ordinal);
                if (required.Count == 0)
                {
                    // تبويبات بلا إجراءات (home/settings) — افتح إن كان الدور يملك التبويب أو أي صلاحية لوحة
                    continue;
                }

                var owned = required.Where(rawNames.Contains).ToList();
                string decision;
                string? reason = null;
                if (owned.Count == required.Count)
                    decision = "open";
                else if (owned.Count == 0)
                    decision = "closed";
                else
                {
                    decision = "review";
                    reason = "partial_legacy_access";
                }

                review.Add(new PanelTabMigrationReviewRow
                {
                    UserId = user.Id,
                    Username = user.Username ?? "",
                    RoleName = roleName,
                    TabKey = tab.TabKey,
                    Decision = decision,
                    OwnedActions = string.Join(',', owned),
                    WouldGainActions = decision == "review"
                        ? string.Join(',', required.Except(owned, StringComparer.Ordinal))
                        : null,
                    Reason = reason
                });

                if (apply == false)
                    continue;

                if (byName.TryGetValue(AdminPanelTabs.PermissionName(tab.TabKey), out var tabPerm) == false)
                    continue;

                var roleHasTab = rolePerms.Any(p => p.Id == tabPerm.Id);
                var wantEnabled = decision == "open";

                // اكتب استثناء فقط عند الاختلاف عن افتراض الدور
                if (wantEnabled == roleHasTab)
                {
                    // inherit — أزل أي تخصيص سابق لهذا التبويب فقط إن وُجد من تشغيل سابق
                    var existingAllow = user.UserPermissions.FirstOrDefault(x => x.PermissionId == tabPerm.Id);
                    var existingDeny = user.UserPermissionDenials.FirstOrDefault(x => x.PermissionId == tabPerm.Id);
                    if (existingAllow is not null)
                    {
                        db.UserPermissions.Remove(existingAllow);
                        overridesWritten++;
                    }

                    if (existingDeny is not null)
                    {
                        db.UserPermissionDenials.Remove(existingDeny);
                        overridesWritten++;
                    }

                    continue;
                }

                if (wantEnabled)
                {
                    var deny = user.UserPermissionDenials.FirstOrDefault(x => x.PermissionId == tabPerm.Id);
                    if (deny is not null)
                        db.UserPermissionDenials.Remove(deny);
                    if (user.UserPermissions.Any(x => x.PermissionId == tabPerm.Id) == false)
                    {
                        db.UserPermissions.Add(new UserPermission(user.Id, tabPerm.Id));
                        overridesWritten++;
                    }
                }
                else
                {
                    var allow = user.UserPermissions.FirstOrDefault(x => x.PermissionId == tabPerm.Id);
                    if (allow is not null)
                        db.UserPermissions.Remove(allow);
                    if (user.UserPermissionDenials.Any(x => x.PermissionId == tabPerm.Id) == false)
                    {
                        db.UserPermissionDenials.Add(new UserPermissionDenial(user.Id, tabPerm.Id));
                        overridesWritten++;
                    }
                }
            }
        }

        var snapshotPath = "";
        if (string.IsNullOrWhiteSpace(snapshotDirectory) == false)
        {
            Directory.CreateDirectory(snapshotDirectory);
            snapshotPath = Path.Combine(
                snapshotDirectory,
                $"panel-tab-snapshot-{DateTime.UtcNow:yyyyMMddHHmmss}.json");
            await File.WriteAllTextAsync(
                snapshotPath,
                System.Text.Json.JsonSerializer.Serialize(snapshot, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                }),
                cancellationToken);
        }

        if (apply)
            await db.SaveChangesAsync(cancellationToken);

        return new PanelTabMigrationResult
        {
            Applied = apply,
            UsersScanned = users.Count,
            OverridesWritten = overridesWritten,
            ReviewRows = review,
            SnapshotPath = snapshotPath
        };
    }
}
