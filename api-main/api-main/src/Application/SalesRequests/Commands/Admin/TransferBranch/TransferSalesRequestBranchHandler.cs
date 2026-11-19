using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.TransferBranch;

public class TransferSalesRequestBranchHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<TransferSalesRequestBranchCommand, Result>
{
    public async Task<Result> Handle(TransferSalesRequestBranchCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var entity = await db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && x.IsDeleted == false, cancellationToken);
        if (entity is null || branches.Contains(entity.BranchId) == false)
            return SalesRequestErrors.NotFound;

        if (request.TargetBranchId == entity.BranchId)
            return SalesRequestErrors.TransferSameBranch;
        if (branches.Contains(request.TargetBranchId) == false)
            return SalesRequestErrors.InvalidBranch;
        if (SalesRequest.CanMoveToBranch(entity) == false)
            return SalesRequestErrors.TransferNotAllowed;

        var target = await db.Branches.AsNoTracking()
            .Where(b => b.Id == request.TargetBranchId)
            .Select(b => new { b.Id, b.ProvinceId })
            .FirstOrDefaultAsync(cancellationToken);
        if (target is null)
            return SalesRequestErrors.InvalidBranch;

        var at = clock.UtcNowOffset;
        if (request.EmployeeId is int employeeId)
        {
            var eligible = await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
                db, employeeId, target.Id, at, cancellationToken);
            if (eligible == false)
                return SalesRequestErrors.InvalidEmployee;
        }

        var reason = request.Reason.Trim();
        var actorId = currentUser.UserId ?? 0;
        var opId = Guid.NewGuid();
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;

        entity.MoveToBranch(target.Id, target.ProvinceId, reason, at);
        SalesRequestTimelineWriter.Append(
            db, entity.Id, SalesRequestTimelineEventType.BranchTransferred, actorId, at,
            prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, reason, opId);
        SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.Invalidation, at, prevEmployee);

        if (request.EmployeeId is int assignee)
        {
            entity.Assign(assignee, at);
            SalesRequestTimelineWriter.Append(
                db, entity.Id, SalesRequestTimelineEventType.Assigned, actorId, at,
                SalesRequestStatus.New, entity.Status, null, entity.TargetEmployeeId, null, opId);
            SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.Assigned, at, null);
        }

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(ct => db.SaveChangesAsync(ct), cancellationToken);
    }
}
