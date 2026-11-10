using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.Admin.RequestException;

public class RequestSalesRequestExceptionHandler : IRequestHandler<RequestSalesRequestExceptionCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RequestSalesRequestExceptionHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(RequestSalesRequestExceptionCommand request, CancellationToken cancellationToken)
    {
        var branches = SalesRequestAccess.AuthorizedBranches(_currentUser);
        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);

        if (entity is null || !branches.Contains(entity.BranchId))
            return SalesRequestErrors.NotFound;

        if (entity.ExceptionHoldStatus is SalesRequestExceptionHoldStatus.Pending
            || (entity.ExceptionHoldStatus is SalesRequestExceptionHoldStatus.Approved
                && entity.ExceptionConsumedAtUtc is null))
        {
            return SalesRequestErrors.ExceptionAlreadyHeld;
        }

        if (entity.Status is SalesRequestStatus.Completed
            or SalesRequestStatus.Rejected
            or SalesRequestStatus.PreparedForSale
            or SalesRequestStatus.InProgress
            or SalesRequestStatus.ConvertedToSale)
        {
            return SalesRequestErrors.ExceptionNotAllowed;
        }

        var at = _clock.UtcNowOffset;
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;
        entity.HoldForException(SalesRequestExceptionHoldStatus.Pending);
        if (!string.IsNullOrWhiteSpace(request.Reason))
            entity.RejectReason = request.Reason;

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, SalesRequestTimelineEventType.ExceptionRequested,
            _currentUser.UserId ?? 0, at,
            prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, request.Reason, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(
            _db, entity, SalesRequestChangeType.Invalidation, at, prevEmployee);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct),
            cancellationToken);
    }
}