using Application.Common.Interfaces;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Common;

public static class SalesRequestExceptionDecider
{
    public const int MaxNoteLength = 500;

    public static async Task<Result> DecideAsync(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        int requestId,
        string reason,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (currentUser.HasPermission(SeedPermissions.SalesRequest.ExceptionDecide) == false)
            return SalesRequestErrors.ActionForbidden;

        var actorId = currentUser.UserId ?? 0;
        if (actorId <= 0)
            return SalesRequestErrors.ActionForbidden;

        var trimmed = (reason ?? string.Empty).Trim();
        if (trimmed.Length > MaxNoteLength)
            return SalesRequestErrors.DecisionNoteTooLong;

        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var entity = await db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == requestId && !x.IsDeleted, cancellationToken);

        if (entity is null || branches.Contains(entity.BranchId) == false)
            return SalesRequestErrors.NotFound;

        var requesterId = await db.SalesRequestTimelineEntries.AsNoTracking()
            .Where(x => x.SalesRequestId == requestId
                        && x.EventType == SalesRequestTimelineEventType.ExceptionRequested)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => (int?)x.ActorUserId)
            .FirstOrDefaultAsync(cancellationToken);

        var check = SalesRequestExtraViewRules.CheckDecision(entity, actorId, requesterId, approve);
        switch (check)
        {
            case ExceptionDecisionCheck.NotPending:
                return SalesRequestErrors.ExceptionNotPending;
            case ExceptionDecisionCheck.TerminalStatus:
                return SalesRequestErrors.IllegalTransition;
            case ExceptionDecisionCheck.RequesterUnknown:
                return SalesRequestErrors.ExceptionRequesterUnknown;
            case ExceptionDecisionCheck.SelfApproval:
                return SalesRequestErrors.ExceptionSelfApproval;
        }

        var at = clock.UtcNowOffset;

        var prevStatus = entity.Status;
        if (approve)
            entity.ApproveException(actorId, at);
        else
        {
            entity.RejectException(actorId, at);
            entity.Reject(trimmed.Length == 0 ? "رفض الاستثناء" : $"رفض الاستثناء: {trimmed}", at, clearAssignee: true);
        }

        entity.ExceptionReadAtUtc ??= at;

        SalesRequestTimelineWriter.Append(
            db, entity.Id,
            approve ? SalesRequestTimelineEventType.ExceptionApproved : SalesRequestTimelineEventType.ExceptionRejected,
            actorId, at,
            prevStatus, entity.Status, entity.TargetEmployeeId, entity.TargetEmployeeId,
            trimmed.Length == 0 ? null : trimmed, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(
            db, entity, SalesRequestChangeType.Invalidation, at, entity.TargetEmployeeId);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => db.SaveChangesAsync(ct),
            cancellationToken);
    }
}
