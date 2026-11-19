using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.Return;

public class ReturnSalesRequestHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IDateTimeProvider clock)
    : IRequestHandler<ReturnSalesRequestCommand, Result>
{
    public async Task<Result> Handle(ReturnSalesRequestCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(currentUser);
        var entity = await db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && x.IsDeleted == false, cancellationToken);
        if (entity is null || branches.Contains(entity.BranchId) == false)
            return SalesRequestErrors.NotFound;

        if (entity.Status != SalesRequestStatus.Rejected)
            return SalesRequestErrors.IllegalTransition;
        if (entity.TargetEmployeeId is not int employeeId || employeeId <= 0)
            return SalesRequestErrors.ReturnRequiresEmployee;

        var at = clock.UtcNowOffset;
        var eligible = await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
            db, employeeId, entity.BranchId, at, cancellationToken);
        if (eligible == false)
            return SalesRequestErrors.InvalidEmployee;

        var prevStatus = entity.Status;
        entity.ReturnToEmployee(employeeId, request.Note, at);
        entity.ViewedAtUtc = null;

        SalesRequestTimelineWriter.Append(
            db, entity.Id, SalesRequestTimelineEventType.Returned, currentUser.UserId ?? 0, at,
            prevStatus, entity.Status, employeeId, entity.TargetEmployeeId, request.Note, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(db, entity, SalesRequestChangeType.StatusChanged, at, employeeId);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(ct => db.SaveChangesAsync(ct), cancellationToken);
    }
}
