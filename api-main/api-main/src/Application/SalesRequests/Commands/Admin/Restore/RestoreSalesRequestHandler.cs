using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;
using SeedPermissions = Application.Common.SeedData.Permissions;

namespace Application.SalesRequests.Commands.Admin.Restore;

public class RestoreSalesRequestHandler : IRequestHandler<RestoreSalesRequestCommand, Result>
{
    public const string AssigneeClearedMarker = "assignee_cleared";
    public const string HoldDowngradedMarker = "approved_exception_downgraded_to_pending";

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RestoreSalesRequestHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(RestoreSalesRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.HasPermission(SeedPermissions.SalesRequest.Restore) == false)
            return SalesRequestErrors.ActionForbidden;

        var actorId = _currentUser.UserId ?? 0;
        if (actorId <= 0)
            return SalesRequestErrors.ActionForbidden;

        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId, cancellationToken);

        if (entity is null || branches.Contains(entity.BranchId) == false)
            return SalesRequestErrors.NotFound;

        var at = _clock.UtcNowOffset;
        var assigneeEligible = true;
        if (entity.TargetEmployeeId is > 0)
        {
            assigneeEligible = await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
                _db, entity.TargetEmployeeId.Value, entity.BranchId, at, cancellationToken);
        }

        var check = SalesRequestExtraViewRules.CheckRestore(entity, assigneeEligible);
        switch (check)
        {
            case RestoreCheck.NotDeleted:
                return SalesRequestErrors.NotDeleted;
            case RestoreCheck.LinkedOrderAssigneeIneligible:
                return SalesRequestErrors.RestoreLinkedAssigneeIneligible;
        }
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;

        var outcome = entity.Restore(assigneeEligible, at);

        var markers = new List<string>();
        if (outcome.AssigneeCleared)
            markers.Add(AssigneeClearedMarker);
        if (outcome.ApprovedHoldDowngraded)
            markers.Add(HoldDowngradedMarker);

        var parts = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Reason) == false)
            parts.Add(request.Reason.Trim());
        if (markers.Count > 0)
            parts.Add($"[{string.Join(",", markers)}]");

        var timelineReason = parts.Count == 0 ? null : string.Join(" ", parts);

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, SalesRequestTimelineEventType.Restored, actorId, at,
            prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, timelineReason, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(
            _db, entity, SalesRequestChangeType.Invalidation, at, prevEmployee);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct),
            cancellationToken);
    }
}
