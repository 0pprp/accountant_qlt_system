using Application.Common.Interfaces;
using Application.SalesRequests.Common;

namespace Application.SalesFollowup.Common;

public static class SalesFollowupAccess
{
    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => SalesRequestAccess.AuthorizedBranches(user);

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => SalesRequestAccess.CanAccessBranch(user, branchId);

    public static Error? ValidateBranch(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
            return SalesFollowupErrors.BranchRequired;
        if (!CanAccessBranch(user, branchId.Value))
            return SalesFollowupErrors.BranchForbidden;
        return null;
    }

    /// <summary>null أو ≤0 = كل الفروع المصرّح بها للمستخدم.</summary>
    public static Error? ValidateBranchOrAll(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
        {
            if (AuthorizedBranches(user).Count == 0)
                return SalesFollowupErrors.BranchForbidden;
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
