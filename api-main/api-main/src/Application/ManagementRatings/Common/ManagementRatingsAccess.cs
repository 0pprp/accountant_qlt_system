using Application.Common.Interfaces;
using Application.SalesRequests.Common;

namespace Application.ManagementRatings.Common;

public static class ManagementRatingsAccess
{
    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => SalesRequestAccess.AuthorizedBranches(user);

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => SalesRequestAccess.CanAccessBranch(user, branchId);

    public static Error? ValidateBranchRequired(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
            return ManagementRatingsErrors.BranchRequired;
        if (CanAccessBranch(user, branchId.Value) == false)
            return ManagementRatingsErrors.BranchAccessDenied;
        return null;
    }

    public static Error? ValidateBranchOrAll(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
        {
            if (AuthorizedBranches(user).Count == 0)
                return ManagementRatingsErrors.BranchAccessDenied;
            return null;
        }

        return ValidateBranchRequired(user, branchId);
    }

    public static IReadOnlyList<int> ResolveBranchScope(ICurrentUserService user, int? branchId)
    {
        var authorized = AuthorizedBranches(user);
        if (branchId is null or <= 0)
            return authorized;
        return authorized.Contains(branchId.Value) ? [branchId.Value] : [];
    }
}
