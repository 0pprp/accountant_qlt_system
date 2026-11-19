using Application.SalesRequests.Common;
using Application.Common.Interfaces;
using Application.FollowerWork.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.FollowerWork.Commands.Admin.DeleteFollower;

public class DeleteFollowerHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IActivityLogService activityLog)
    : IRequestHandler<DeleteFollowerCommand, Result>
{
    public async Task<Result> Handle(DeleteFollowerCommand request, CancellationToken ct)
    {
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.ManageFollowers) == false)
            return FollowerWorkErrors.ManageFollowersDenied;

        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(r => r.Role)
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.Id == request.FollowerId, ct);
        if (user is null || user.UserRoles.Any(r => r.Role != null && r.Role.Name == SeedRoles.Motaba.Name) == false)
            return FollowerWorkErrors.FollowerAccountNotFound;

        var branches = user.UserBranches.Select(b => b.BranchId).ToList();
        if (branches.Count == 0 || branches.Any(b => FollowerWorkAccess.CanAccessBranch(currentUser, b) == false))
            return FollowerWorkErrors.BranchAccessDenied;

        if (await db.OrderLists.AnyAsync(l => l.MotabaId == user.Id, ct))
            return FollowerWorkErrors.FollowerHasLists;

        var hasHistory = await db.FollowerSaleAuthentications.AnyAsync(a => a.FollowerUserId == user.Id, ct)
                         || await db.ActivityLogs.AnyAsync(a => a.UserId == user.Id, ct);
        if (hasHistory)
            return FollowerWorkErrors.FollowerHasHistory;

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        foreach (var branchId in branches)
        {
            await activityLog.AddAsync(
                branchId,
                ActivityType.UserUpdated,
                TargetEntityType.User,
                user.Id,
                details: $"follower deleted; username={user.Username}");
        }

        db.Users.Remove(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return FollowerWorkErrors.FollowerHasHistory;
        }

        await tx.CommitAsync(ct);
        return Result.Success();
    }
}
