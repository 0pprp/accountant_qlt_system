using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.Assign;

public class AssignSalesRequestHandler : IRequestHandler<AssignSalesRequestCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public AssignSalesRequestHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(AssignSalesRequestCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);

        if (entity is null || !branches.Contains(entity.BranchId))
            return SalesRequestErrors.NotFound;

        var gate = SalesRequestExtraViewRules.AssignGate(entity);
        if (gate == AssignExceptionGate.BlockedPending)
            return SalesRequestErrors.ExceptionPendingBlocksAssign;
        if (gate == AssignExceptionGate.BlockedRejected)
            return SalesRequestErrors.ExceptionRejectedBlocksAssign;
        var consumesHold = gate == AssignExceptionGate.ConsumesApprovedHold;

        var at = _clock.UtcNowOffset;
        var employeeOk = await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
            _db, request.EmployeeId, entity.BranchId, at, cancellationToken);
        if (!employeeOk)
            return SalesRequestErrors.InvalidEmployee;
        var opId = Guid.NewGuid();
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;
        var actorId = _currentUser.UserId ?? 0;

        var firstAssignment = entity.IsUnassigned()
                              || (consumesHold
                                  && entity.TargetEmployeeId is null or <= 0
                                  && SalesRequestExtraViewRules.IsTerminal(entity.Status) == false);

        if (firstAssignment)
        {
            entity.Assign(request.EmployeeId, at);
            SalesRequestTimelineWriter.Append(
                _db, entity.Id, SalesRequestTimelineEventType.Assigned, actorId, at,
                prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, null, opId);
            SalesRequestChangeOutboxWriter.Append(
                _db, entity, SalesRequestChangeType.Assigned, at, prevEmployee);
        }
        else if (entity.CanEmployeeTransition() || entity.Status == SalesRequestStatus.Rejected)
        {
            // إعادة إسناد مصرّح بها إداريًا ضمن النطاق
            entity.Reassign(request.EmployeeId, request.Reason, at);
            SalesRequestTimelineWriter.Append(
                _db, entity.Id, SalesRequestTimelineEventType.Reassigned, actorId, at,
                prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, request.Reason, opId);
            SalesRequestChangeOutboxWriter.Append(
                _db, entity, SalesRequestChangeType.Reassigned, at, prevEmployee);
        }
        else
        {
            return SalesRequestErrors.IllegalTransition;
        }

        if (consumesHold)
        {
            SalesRequestTimelineWriter.Append(
                _db, entity.Id, SalesRequestTimelineEventType.ExceptionConsumed, actorId, at,
                prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, null, opId);
        }

        // لا إنشاء Order/Customer عند الإسناد
        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct),
            cancellationToken);
    }
}
