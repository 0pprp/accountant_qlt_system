using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.ManagerProgress;

public class ManagerProgressSalesRequestHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<ManagerProgressSalesRequestCommand, Result>
{
    public async Task<Result> Handle(ManagerProgressSalesRequestCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var entity = await db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && x.IsDeleted == false, cancellationToken);
        if (entity is null || branches.Contains(entity.BranchId) == false)
            return SalesRequestErrors.NotFound;

        if (entity.TargetEmployeeId is null or <= 0
            || entity.HasLiveExceptionHold
            || ManagerProgressRules.IsAllowed(request.Action, entity.Status) == false)
            return SalesRequestErrors.IllegalTransition;

        var at = clock.UtcNowOffset;
        var prevStatus = entity.Status;
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        SalesRequestTimelineEventType eventType;
        if (request.Action == ManagerProgressAction.Prepare)
        {
            entity.Prepare(note, at);
            eventType = SalesRequestTimelineEventType.Prepared;
        }
        else
        {
            entity.Pend(note!, at);
            eventType = SalesRequestTimelineEventType.Pend;
        }

        SalesRequestTimelineWriter.Append(
            db, entity.Id, eventType, currentUser.UserId ?? 0, at,
            prevStatus, entity.Status, entity.TargetEmployeeId, entity.TargetEmployeeId, note, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.StatusChanged, at, entity.TargetEmployeeId);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(ct => db.SaveChangesAsync(ct), cancellationToken);
    }
}
