using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.LegalFollowupAggregate.Enums;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.CancelToUnassigned;

public class CancelSalesRequestToUnassignedHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<CancelSalesRequestToUnassignedCommand, Result>
{
    public async Task<Result> Handle(CancelSalesRequestToUnassignedCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var entity = await db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && x.IsDeleted == false, cancellationToken);
        if (entity is null || branches.Contains(entity.BranchId) == false)
            return SalesRequestErrors.NotFound;

        if (SalesRequest.CanCancelToUnassigned(entity) == false)
            return SalesRequestErrors.CancelNotAllowed;

        var at = clock.UtcNowOffset;
        var reason = request.Reason.Trim();
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;

        var openCases = await db.LegalFollowupCases
            .Where(c => c.SalesRequestId == entity.Id && c.Status == LegalFollowupStatus.InProgress)
            .ToListAsync(cancellationToken);
        foreach (var legalCase in openCases)
            legalCase.Cancel(reason, at, null);

        entity.CancelToUnassigned(reason, at);

        SalesRequestTimelineWriter.Append(
            db, entity.Id, SalesRequestTimelineEventType.CancelledToUnassigned, currentUser.UserId ?? 0, at,
            prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, reason, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.StatusChanged, at, prevEmployee);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(ct => db.SaveChangesAsync(ct), cancellationToken);
    }
}
