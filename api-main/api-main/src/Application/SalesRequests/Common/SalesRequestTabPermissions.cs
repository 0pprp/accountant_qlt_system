using Application.Common.Interfaces;
using Domain.Services.SalesRequestLifecycle;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Common;

/// <summary>صلاحية قراءة كل تبويب — مستقلة عن أكشنات Assign/Reject.</summary>
public static class SalesRequestTabPermissions
{
    public static string? PermissionForTab(string tab) =>
        SalesRequestTabMap.Normalize(tab) switch
        {
            SalesRequestTabMap.Unassigned => SeedPermissions.SalesRequest.ReadUnassigned,
            SalesRequestTabMap.Incoming => SeedPermissions.SalesRequest.TabIncoming,
            SalesRequestTabMap.Prepared => SeedPermissions.SalesRequest.TabPrepared,
            SalesRequestTabMap.Inspected => SeedPermissions.SalesRequest.TabInspected,
            SalesRequestTabMap.Pending => SeedPermissions.SalesRequest.TabPending,
            SalesRequestTabMap.Rejected => SeedPermissions.SalesRequest.TabRejected,
            SalesRequestTabMap.Sold => SeedPermissions.SalesRequest.TabSold,
            SalesRequestTabMap.Sent => SeedPermissions.SalesRequest.TabSent,
            SalesRequestTabMap.Exceptions => SeedPermissions.SalesRequest.TabExceptions,
            SalesRequestTabMap.Deleted => SeedPermissions.SalesRequest.TabDeleted,
            SalesRequestTabMap.Legal => SeedPermissions.SalesRequest.TabLegal,
            _ => null
        };

    public static bool CanReadTab(ICurrentUserService user, string tab)
    {
        var perm = PermissionForTab(tab);
        return perm != null && user.HasPermission(perm);
    }

    /// <summary>تبويبات سير العمل المسموح قراءتها — أساس اتحاد «الكل».</summary>
    public static IReadOnlyList<string> AllowedWorkflowTabs(ICurrentUserService user)
        => SalesRequestTabMap.WorkflowTabs.Where(t => CanReadTab(user, t)).ToList();

    public static IReadOnlyList<string> AllowedExtraViews(ICurrentUserService user)
        => SalesRequestTabMap.ExtraViewsImplemented.Where(t => CanReadTab(user, t)).ToList();

    public static bool HasAnyReadableView(ICurrentUserService user)
        => AllowedWorkflowTabs(user).Count > 0 || AllowedExtraViews(user).Count > 0;
}
