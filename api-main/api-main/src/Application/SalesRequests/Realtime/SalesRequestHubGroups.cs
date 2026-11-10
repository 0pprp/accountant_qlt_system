using Application.SalesRequests.Common;
using Domain.Services.SalesRequestLifecycle;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Realtime;

public sealed record SalesRequestHubIdentity(
    int UserId,
    IReadOnlyList<int> BranchIds,
    IReadOnlySet<string> Permissions);

public static class SalesRequestHubGroups
{
    public const string Prefix = "sr:";

    public static string User(int userId) => $"{Prefix}user:{userId}";

    public static string BranchAdmin(int branchId) => $"{Prefix}branch-admin:{branchId}";

    public static bool CanReceiveEmployeeEvents(IReadOnlySet<string> permissions)
        => permissions.Contains(SeedPermissions.SalesRequest.AppRead);

    public static bool CanReceiveBranchEvents(IReadOnlySet<string> permissions)
        => SalesRequestTabMap.WorkflowTabs.Concat(SalesRequestTabMap.ExtraViewsImplemented).Any(tab =>
        {
            var permission = SalesRequestTabPermissions.PermissionForTab(tab);
            return permission is not null && permissions.Contains(permission);
        });

    public static IReadOnlySet<string> ForIdentity(SalesRequestHubIdentity identity)
    {
        var groups = new HashSet<string>(StringComparer.Ordinal);

        if (CanReceiveEmployeeEvents(identity.Permissions))
            groups.Add(User(identity.UserId));

        if (CanReceiveBranchEvents(identity.Permissions))
        {
            foreach (var branchId in identity.BranchIds)
                groups.Add(BranchAdmin(branchId));
        }

        return groups;
    }

    public static IReadOnlyCollection<string> ForChange(
        int branchId,
        int? targetEmployeeId,
        int? previousTargetEmployeeId)
    {
        var groups = new List<string> { BranchAdmin(branchId) };

        if (targetEmployeeId is > 0)
            groups.Add(User(targetEmployeeId.Value));

        if (previousTargetEmployeeId is > 0 && previousTargetEmployeeId != targetEmployeeId)
            groups.Add(User(previousTargetEmployeeId.Value));

        return groups;
    }
}
