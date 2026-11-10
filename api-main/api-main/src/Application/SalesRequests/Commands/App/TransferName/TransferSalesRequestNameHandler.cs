using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.App.TransferName;

public sealed class TransferSalesRequestNameHandler
    : IRequestHandler<TransferSalesRequestNameCommand, Result<SalesRequestAppItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public TransferSalesRequestNameHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SalesRequestAppItemDto>> Handle(
        TransferSalesRequestNameCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return SalesRequestErrors.NotOwner;

        var reason = (request.Reason ?? string.Empty).Trim();
        if (reason.Length < 3)
            return SalesRequestErrors.NoteRequired;

        if (request.ToEmployeeId == userId.Value)
            return SalesRequestErrors.TransferSelfForbidden;

        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);
        if (entity is null)
            return SalesRequestErrors.NotFound;
        if (!entity.IsOwnedBy(userId.Value))
            return SalesRequestErrors.NotOwner;

        if (entity.Status is SalesRequestStatus.Completed
            or SalesRequestStatus.Rejected
            or SalesRequestStatus.LegalFollowup)
            return SalesRequestErrors.TransferTerminalForbidden;

        if (!entity.CanEmployeeTransition() && entity.Status != SalesRequestStatus.Rejected)
            return SalesRequestErrors.IllegalTransition;

        var at = _clock.UtcNowOffset;
        var peerOk = await SalesRequestEmployeeEligibility.IsEligibleAssigneeAsync(
            _db, request.ToEmployeeId, entity.BranchId, at, cancellationToken);
        if (!peerOk)
            return SalesRequestErrors.TransferPeerInvalid;
        var prevStatus = entity.Status;
        var prevEmployee = entity.TargetEmployeeId;
        var opId = Guid.NewGuid();

        try
        {
            entity.Reassign(request.ToEmployeeId, reason, at);
        }
        catch (InvalidOperationException)
        {
            return SalesRequestErrors.IllegalTransition;
        }

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, SalesRequestTimelineEventType.Reassigned, userId.Value, at,
            prevStatus, entity.Status, prevEmployee, entity.TargetEmployeeId, reason, opId);
        SalesRequestChangeOutboxWriter.Append(
            _db, entity, SalesRequestChangeType.Reassigned, at, prevEmployee);

        var save = await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct), cancellationToken);
        if (save.IsFailed)
            return save.Error!;

        return new SalesRequestAppItemDto
        {
            RequestId = entity.Id,
            CustomerName = entity.CustomerName,
            CustomerPhone = entity.CustomerPhone,
            Status = entity.Status,
            StatusLabel = SalesRequestStatusLabels.For(entity.Status),
            TabKey = SalesRequestTabMap.DisplayTabForStatus(entity.Status),
            SourceType = entity.SourceType,
            SourceTypeLabel = SalesRequestSourceLabels.For(entity.SourceType),
            BranchId = entity.BranchId,
            TargetEmployeeId = entity.TargetEmployeeId,
            LinkedOrderId = entity.LinkedOrderId,
            CreatedAt = entity.CreatedAt,
            StatusChangedAtUtc = entity.StatusChangedAtUtc,
            ViewedAtUtc = entity.ViewedAtUtc,
            LastStatusNote = entity.LastStatusNote,
            IsReturned = entity.Status == SalesRequestStatus.Returned
        };
    }
}
