using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.Reject;

public class RejectSalesRequestHandler : IRequestHandler<RejectSalesRequestCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RejectSalesRequestHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(RejectSalesRequestCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);

        if (entity is null || !branches.Contains(entity.BranchId))
            return SalesRequestErrors.NotFound;

        if (entity.Status == SalesRequestStatus.Completed)
            return SalesRequestErrors.IllegalTransition;

        var at = _clock.UtcNowOffset;
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;
        entity.Reject(request.Reason, at, clearAssignee: entity.IsUnassigned());

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, SalesRequestTimelineEventType.Rejected,
            _currentUser.UserId ?? 0, at,
            prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, request.Reason, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(
            _db, entity, SalesRequestChangeType.StatusChanged, at, prevEmployee);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct),
            cancellationToken);
    }
}
