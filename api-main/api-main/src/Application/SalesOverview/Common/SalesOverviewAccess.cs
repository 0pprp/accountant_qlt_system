using Application.Common.Interfaces;
using Application.SalesRequests.Common;

namespace Application.SalesOverview.Common;

public static class SalesOverviewAccess
{
    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => SalesRequestAccess.AuthorizedBranches(user);

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => SalesRequestAccess.CanAccessBranch(user, branchId);

    public static Error? ValidateBranchOrAll(ICurrentUserService user, int? branchId)
    {
        if (AuthorizedBranches(user).Count == 0)
            return SalesOverviewErrors.NoBranchScope;
        if (branchId is null or <= 0)
            return null;
        if (!CanAccessBranch(user, branchId.Value))
            return SalesOverviewErrors.BranchForbidden;
        return null;
    }

    public static IReadOnlyList<int> ResolveBranchScope(ICurrentUserService user, int? branchId)
    {
        var authorized = AuthorizedBranches(user);
        if (branchId is null or <= 0)
            return authorized;
        return authorized.Contains(branchId.Value) ? [branchId.Value] : [];
    }
}
