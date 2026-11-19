using Application.Common.Interfaces;
using Application.SalesRequests.Commands.App.Submit;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.Update;

public class UpdateSalesRequestHandler : IRequestHandler<UpdateSalesRequestCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public UpdateSalesRequestHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(UpdateSalesRequestCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);

        if (entity is null || !branches.Contains(entity.BranchId))
            return SalesRequestErrors.NotFound;

        if (entity.Status == SalesRequestStatus.Completed)
            return SalesRequestErrors.UpdateCompletedForbidden;

        if (!branches.Contains(request.BranchId))
            return SalesRequestErrors.InvalidBranch;

        var branch = await _db.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.BranchId, cancellationToken);
        if (branch is null)
            return SalesRequestErrors.InvalidBranch;

        var provinceId = request.ProvinceId ?? branch.ProvinceId;
        if (provinceId != branch.ProvinceId)
            return SalesRequestErrors.InvalidBranch;

        var at = _clock.UtcNowOffset;
        var actorId = _currentUser.UserId ?? 0;
        var opId = Guid.NewGuid();
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;
        var prevBranch = entity.BranchId;
        var phone = SalesRequestSubmissionRules.NormalizePhone(request.CustomerPhone);

        entity.UpdateDetails(
            request.CustomerName,
            phone,
            request.Address,
            request.RequestedProduct,
            request.BranchId,
            provinceId);

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, SalesRequestTimelineEventType.DetailsUpdated, actorId, at,
            prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId,
            request.Reason, opId);

        var targetEmployeeId = request.EmployeeId;
        var branchMoved = prevBranch != entity.BranchId;

        if (targetEmployeeId is int empId && empId > 0 && (prevEmployee != empId || branchMoved))
        {
            var gate = SalesRequestExtraViewRules.AssignGate(entity);
            if (gate == AssignExceptionGate.BlockedPending)
                return SalesRequestErrors.ExceptionPendingBlocksAssign;
            if (gate == AssignExceptionGate.BlockedRejected)
                return SalesRequestErrors.ExceptionRejectedBlocksAssign;
            var consumesHold = gate == AssignExceptionGate.ConsumesApprovedHold;

            var employeeOk = await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
                _db, empId, entity.BranchId, at, cancellationToken);
            if (!employeeOk)
                return SalesRequestErrors.InvalidEmployee;

            if (prevEmployee != empId)
            {
                var firstAssignment = entity.IsUnassigned()
                                      || (consumesHold
                                          && entity.TargetEmployeeId is null or <= 0
                                          && SalesRequestExtraViewRules.IsTerminal(entity.Status) == false);

                if (firstAssignment)
                {
                    entity.Assign(empId, at);
                    SalesRequestTimelineWriter.Append(
                        _db, entity.Id, SalesRequestTimelineEventType.Assigned, actorId, at,
                        prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, null, opId);
                    SalesRequestChangeOutboxWriter.Append(
                        _db, entity, SalesRequestChangeType.Assigned, at, prevEmployee);
                }
                else if (entity.CanEmployeeTransition() || entity.Status == SalesRequestStatus.Rejected)
                {
                    entity.Reassign(empId, request.Reason, at);
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
            }
            else
            {
                // نفس الموظف — إشعار فوري بتحديث الحقول
                SalesRequestChangeOutboxWriter.Append(
                    _db, entity, SalesRequestChangeType.NoteChanged, at, prevEmployee);
            }
        }
        else if (branchMoved && prevEmployee is int oldEmp && oldEmp > 0)
        {
            var stillEligible = await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
                _db, oldEmp, entity.BranchId, at, cancellationToken);
            if (!stillEligible)
                return SalesRequestErrors.AssigneeRequiredForBranchMove;

            SalesRequestChangeOutboxWriter.Append(
                _db, entity, SalesRequestChangeType.NoteChanged, at, prevEmployee);
        }
        else
        {
            SalesRequestChangeOutboxWriter.Append(
                _db, entity, SalesRequestChangeType.NoteChanged, at, prevEmployee);
        }

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct),
            cancellationToken);
    }
}
