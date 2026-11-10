using Application.Common.Interfaces;
using Application.SalesRequests.Common;

namespace Application.DelegateFollowup.Common;

public static class DelegateFollowupAccess
{
    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => SalesRequestAccess.AuthorizedBranches(user);

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => SalesRequestAccess.CanAccessBranch(user, branchId);

    public static Error? ValidateBranch(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
            return DelegateFollowupErrors.BranchRequired;
        if (CanAccessBranch(user, branchId.Value) == false)
            return DelegateFollowupErrors.BranchForbidden;
        return null;
    }

    public static Error? ValidateBranchOrAll(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
        {
            if (AuthorizedBranches(user).Count == 0)
                return DelegateFollowupErrors.BranchForbidden;
            return null;
        }

        return ValidateBranch(user, branchId);
    }

    public static IReadOnlyList<int> ResolveBranchScope(ICurrentUserService user, int? branchId)
    {
        var authorized = AuthorizedBranches(user);
        if (branchId is null or <= 0)
            return authorized;
        return authorized.Contains(branchId.Value) ? [branchId.Value] : [];
    }
}
