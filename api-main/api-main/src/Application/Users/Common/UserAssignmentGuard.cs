using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.Users.Common;

/// <summary>
/// سياسة المرحلة 02: تغيير الدور/الفروع للسوبر أدمن الحقيقي فقط.
/// التفويض لمديري الفروع مؤجّل — البنية تسمح بإضافة قواعد لاحقًا.
/// </summary>
public static class UserAssignmentGuard
{
    public static async Task<bool> IsRealSuperAdminAsync(
        IApplicationDbContext dbContext,
        int actorUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles.AsNoTracking()
            .AnyAsync(
                x => x.UserId == actorUserId && x.Role!.Name == SeedRoles.Admin.Name,
                cancellationToken);
    }

    public static async Task<Result> EnsureCanAssignRoleAndBranchesAsync(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        int? targetUserId,
        int requestedRoleId,
        IReadOnlyCollection<int> requestedBranchIds,
        int? currentRoleId,
        IReadOnlyCollection<int> currentBranchIds,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is null)
            return UserErrors.NotAuthorizedToAssignRoleOrBranches;

        var actorId = currentUserService.UserId.Value;
        var isSuperAdmin = await IsRealSuperAdminAsync(dbContext, actorId, cancellationToken);
        if (isSuperAdmin == false)
            return UserErrors.NotAuthorizedToAssignRoleOrBranches;

        var role = await dbContext.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == requestedRoleId, cancellationToken);
        if (role is null)
            return UserErrors.RoleNotFound;

        if (role.Name == SeedRoles.Admin.Name)
            return UserErrors.CannotAssignAdminRole;

        if (targetUserId is int tid)
        {
            if (actorId == tid)
                return UserErrors.CannotManageOwnPermissions;

            var targetIsProtected = await dbContext.UserRoles.AsNoTracking()
                .AnyAsync(
                    x => x.UserId == tid && x.Role!.Name == SeedRoles.Admin.Name,
                    cancellationToken);
            if (targetIsProtected)
                return UserErrors.ProtectedAccount;
        }

        var distinctBranches = requestedBranchIds.Distinct().ToList();
        if (distinctBranches.Count == 0)
            return UserErrors.BranchesRequired;

        var existingBranches = await dbContext.Branches
            .CountAsync(x => distinctBranches.Contains(x.Id), cancellationToken);
        if (existingBranches != distinctBranches.Count)
            return UserErrors.SomeBranchesNotFound;

        // سياسة مستقبلية: تقييد الفروع ضمن نطاق الفاعل — غير مفعّلة للسوبر أدمن.
        _ = currentRoleId;
        _ = currentBranchIds;

        return Result.Success();
    }

    public static bool RoleOrBranchesChanged(
        int? currentRoleId,
        IReadOnlyCollection<int> currentBranchIds,
        int requestedRoleId,
        IReadOnlyCollection<int> requestedBranchIds)
    {
        if (currentRoleId != requestedRoleId)
            return true;

        var current = currentBranchIds.OrderBy(x => x).ToArray();
        var requested = requestedBranchIds.Distinct().OrderBy(x => x).ToArray();
        if (current.Length != requested.Length)
            return true;

        return current.SequenceEqual(requested) == false;
    }
}
