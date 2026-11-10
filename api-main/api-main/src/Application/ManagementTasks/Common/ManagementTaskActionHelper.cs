using Application.Common.Interfaces;
using Application.ManagementTasks.Realtime;
using Application.SalesRequests.Common;
using Domain.Entities.ActivityLogAggregate.Enums;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.ManagementTasks.Common;

public sealed record ManagementTaskActionContext(ManagementTask Task, int ActorId, string ActorName);

public static class ManagementTaskActionHelper
{
    public static Error MapTransitionError(string? error)
    {
        if (string.Equals(error, ManagementTaskErrors.RejectNoteRequired.Message, StringComparison.Ordinal))
            return ManagementTaskErrors.RejectNoteRequired;

        return ManagementTaskErrors.InvalidTransition;
    }

    public static async Task<(ManagementTaskActionContext? Context, Error? Error)> PrepareAsync(
        IApplicationDbContext db,
        ICurrentUserService user,
        string permission,
        int taskId,
        int expectedVersion,
        CancellationToken cancellationToken)
    {
        if (user.HasPermission(permission) == false)
            return (null, ManagementTaskErrors.ActionForbidden);

        var actorId = user.UserId ?? 0;
        if (actorId <= 0)
            return (null, ManagementTaskErrors.ActionForbidden);

        var task = await db.ManagementTasks
            .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

        if (task is null || ManagementTaskAccess.CanAccessBranch(user, task.BranchId) == false)
            return (null, ManagementTaskErrors.NotFound);

        if (task.MatchesExpectedVersion(expectedVersion) == false)
            return (null, ManagementTaskErrors.ConcurrencyConflict);

        var actorName = await ResolveUserNameAsync(db, actorId, cancellationToken);
        if (actorName is null)
            return (null, ManagementTaskErrors.ActionForbidden);

        return (new ManagementTaskActionContext(task, actorId, actorName), null);
    }

    public static async Task<string?> ResolveUserNameAsync(
        IApplicationDbContext db,
        int userId,
        CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<Error?> ValidateLinkedEntitiesAsync(
        IApplicationDbContext db,
        int branchId,
        int? customerId,
        int? orderId,
        CancellationToken cancellationToken)
    {
        if (customerId is > 0)
        {
            var customerOk = await db.Customers
                .AsNoTracking()
                .AnyAsync(x => x.Id == customerId.Value && x.BranchId == branchId, cancellationToken);
            if (customerOk == false)
                return ManagementTaskErrors.LinkedEntityBranchMismatch;
        }

        if (orderId is > 0)
        {
            var orderOk = await db.Orders
                .AsNoTracking()
                .AnyAsync(x => x.Id == orderId.Value && x.BranchId == branchId, cancellationToken);
            if (orderOk == false)
                return ManagementTaskErrors.LinkedEntityBranchMismatch;
        }

        return null;
    }

    public static int? NormalizeLink(int? id) => id is > 0 ? id : null;

    public static async Task<Result<ManagementTaskActionResultDto>> CommitAsync(
        IApplicationDbContext db,
        IActivityLogService activityLog,
        ManagementTaskActionContext context,
        ManagementTaskStatus previousStatus,
        ManagementTaskEventType eventType,
        ManagementTaskChangeType changeType,
        DateTimeOffset atUtc,
        int? targetUserId,
        string? targetDisplayName,
        string? note,
        CancellationToken cancellationToken)
    {
        var task = context.Task;

        ManagementTaskEventWriter.Append(
            db,
            task,
            eventType,
            atUtc,
            context.ActorId,
            context.ActorName,
            previousStatus,
            task.Status,
            targetUserId,
            targetDisplayName,
            note);

        ManagementTaskChangeOutboxWriter.Append(db, task, changeType, atUtc);

        await activityLog.AddAsync(
            task.BranchId,
            ActivityType.ManagementTaskUpdated,
            TargetEntityType.ManagementTask,
            task.Id,
            null,
            $"{eventType}: {task.Title} | {previousStatus} -> {task.Status}");

        var saved = await ManagementTaskConcurrency.SaveWithConflictGuardAsync(
            ct => db.SaveChangesAsync(ct),
            cancellationToken);

        if (saved.IsFailed)
            return saved.Error!.Value;

        return ManagementTaskMapper.ToActionResult(task);
    }

    public static bool CanUseAnyActionPermission(ICurrentUserService user)
    {
        return user.HasPermission(SeedPermissions.ManagementTask.Accept)
               || user.HasPermission(SeedPermissions.ManagementTask.Reject)
               || user.HasPermission(SeedPermissions.ManagementTask.Complete)
               || user.HasPermission(SeedPermissions.ManagementTask.Confirm)
               || user.HasPermission(SeedPermissions.ManagementTask.Transfer);
    }
}
