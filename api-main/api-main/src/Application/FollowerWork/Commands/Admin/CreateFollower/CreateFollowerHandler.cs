using Application.SalesRequests.Common;
using Application.Common.Interfaces;
using Application.Common.Utilities;
using Application.FollowerWork.Commands.Admin.ManageFollower;
using Application.FollowerWork.Common;
using Application.Users.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.BranchAggregate;
using Domain.Entities.RoleAggregate;
using Domain.Entities.UserAggregate;
using Domain.Entities.UserAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.FollowerWork.Commands.Admin.CreateFollower;

public class CreateFollowerHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IActivityLogService activityLog)
    : IRequestHandler<CreateFollowerCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateFollowerCommand request, CancellationToken ct)
    {
        if (currentUser.HasPermission(SeedPermissions.FollowerWork.ManageFollowers) == false)
            return FollowerWorkErrors.ManageFollowersDenied;

        var branchIds = request.BranchIds.Distinct().ToList();
        if (branchIds.Any(b => FollowerWorkAccess.CanAccessBranch(currentUser, b) == false))
            return FollowerWorkErrors.BranchAccessDenied;

        var roleId = await FollowerAccountRules.FollowerRoleIdAsync(db, ct);
        if (roleId is null)
            return FollowerWorkErrors.FollowerAccountNotFound;

        var assignment = await UserAssignmentGuard.EnsureCanAssignRoleAndBranchesAsync(
            db, currentUser, null, roleId.Value, branchIds, null, [], ct);
        if (assignment.IsFailed)
            return assignment.Error!.Value;

        var username = request.Username.Trim();
        if (await db.Users.AnyAsync(u => u.Username == username, ct))
            return FollowerWorkErrors.UsernameTaken;

        var lists = await FollowerAccountRules.LoadAssignableListsAsync(db, request.ListIds, branchIds, ct);
        if (lists.IsFailed)
            return lists.Error!.Value;

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var user = new User(
            FollowerAccountRules.NormalizeName(request.FullName),
            null,
            null,
            null,
            username,
            PasswordHash.HashPassword(request.Password),
            null,
            FollowerAccountRules.NormalizeOptional(request.PhoneNumber))
        {
            CreationStep = UserCreationStep.Completed,
            UserBranches = branchIds.Select(b => new UserBranch(b)).ToHashSet(),
            UserRoles = [new UserRole(roleId.Value)]
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        foreach (var list in lists.Data!)
            list.Update(list.Name, list.MandobId, user.Id, list.BranchId);

        foreach (var branchId in branchIds)
        {
            await activityLog.AddAsync(
                branchId,
                ActivityType.UserCreated,
                TargetEntityType.User,
                user.Id,
                details: $"follower; lists=[{string.Join(',', lists.Data!.Select(l => l.Id))}]");
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return user.Id;
    }
}
