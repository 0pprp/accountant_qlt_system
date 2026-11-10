using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Domain.Entities.ManagementTaskAggregate;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Common;

public static class ManagementTaskAccess
{
    public static IReadOnlyList<int> AuthorizedBranches(ICurrentUserService user)
        => SalesRequestAccess.AuthorizedBranches(user);

    public static bool CanAccessBranch(ICurrentUserService user, int branchId)
        => SalesRequestAccess.CanAccessBranch(user, branchId);

    public static Error? ValidateBranch(ICurrentUserService user, int? branchId)
    {
        if (branchId is null or <= 0)
            return ManagementTaskErrors.BranchRequired;
        if (CanAccessBranch(user, branchId.Value) == false)
            return ManagementTaskErrors.BranchForbidden;
        return null;
    }

    public static IReadOnlyList<int> ResolveBranchScope(ICurrentUserService user, int? branchId)
    {
        var authorized = AuthorizedBranches(user);
        if (branchId is null or <= 0)
            return authorized;
        return authorized.Contains(branchId.Value) ? [branchId.Value] : [];
    }

    public static bool CanSeeTask(ICurrentUserService user, ManagementTask task)
    {
        if (CanAccessBranch(user, task.BranchId) == false)
            return false;

        var userId = user.UserId ?? 0;
        if (userId <= 0)
            return false;

        if (user.HasPermission(SeedPermissions.ManagementTask.Read))
            return true;

        return task.AssigneeUserId == userId
            || task.CurrentHolderUserId == userId
            || task.CreatedByUserId == userId;
    }

    public static async Task<(Domain.Entities.UserAggregate.User? User, Error? Error)> ResolveEligibleUserAsync(
        IApplicationDbContext db,
        int userId,
        int branchId,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
            return (null, ManagementTaskErrors.AssigneeRequired);

        var user = await db.Users
            .AsNoTracking()
            .Include(x => x.UserBranches)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
            return (null, ManagementTaskErrors.AssigneeNotEligible);

        if (user.UserBranches.Any(b => b.BranchId == branchId) == false)
            return (null, ManagementTaskErrors.AssigneeNotEligible);

        return (user, null);
    }
}
