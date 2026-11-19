using Application.SalesRequests.Common;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.FollowerWork.Commands.Admin.ManageFollower;
using Application.FollowerWork.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;
using SeedRoles = Application.Common.SeedData.Roles;

namespace Application.FollowerWork.Commands.Admin.UpdateFollower;

public class UpdateFollowerHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IActivityLogService activityLog)
    : IRequestHandler<UpdateFollowerCommand, Result>
{
    public async Task<Result> Handle(UpdateFollowerCommand request, CancellationToken ct)
    {
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.ManageFollowers) == false)
            return FollowerWorkErrors.ManageFollowersDenied;

        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(r => r.Role)
            .Include(u => u.UserBranches)
            .FirstOrDefaultAsync(u => u.Id == request.FollowerId, ct);
        if (user is null || user.UserRoles.Any(r => r.Role != null && r.Role.Name == SeedRoles.Motaba.Name) == false)
            return FollowerWorkErrors.FollowerAccountNotFound;

        var followerBranches = user.UserBranches.Select(b => b.BranchId).ToList();
        var allowed = followerBranches.Where(b => FollowerWorkAccess.CanAccessBranch(currentUser, b)).ToList();
        if (allowed.Count == 0 || allowed.Count != followerBranches.Count)
            return FollowerWorkErrors.BranchAccessDenied;

        var lists = await FollowerAccountRules.LoadAssignableListsAsync(db, request.AssignListIds, allowed, ct);
        if (lists.IsFailed)
            return lists.Error!.Value;

        user.Update(
            FollowerAccountRules.NormalizeName(request.FullName),
            user.MotherName,
            user.NationalCode,
            user.BirthDate,
            user.Username,
            user.Address,
            string.IsNullOrWhiteSpace(request.PhoneNumber) ? user.PhoneNumber : FollowerAccountRules.NormalizeOptional(request.PhoneNumber),
            user.UserRoles,
            user.UserBranches);

        var passwordChanged = string.IsNullOrEmpty(request.NewPassword) == false;
        if (passwordChanged)
        {
            user.PasswordHash = PasswordHash.HashPassword(request.NewPassword!);
            user.InvalidateSessions();
        }

        var moved = new List<int>();
        foreach (var list in lists.Data!)
        {
            if (list.MotabaId == user.Id)
                continue;
            list.Update(list.Name, list.MandobId, user.Id, list.BranchId);
            moved.Add(list.Id);
        }

        foreach (var branchId in allowed)
        {
            await activityLog.AddAsync(
                branchId,
                ActivityType.UserUpdated,
                TargetEntityType.User,
                user.Id,
                details: $"follower; passwordChanged={passwordChanged}; movedLists=[{string.Join(',', moved)}]");
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
