using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Application.SalesRequests.Queries.App.GetByTab;
using Domain.Entities.SalesRequestAggregate.Enums;
using Domain.Services.SalesRequestLifecycle;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.App.LinkDraft;

public class LinkSalesRequestDraftHandler
    : IRequestHandler<LinkSalesRequestDraftCommand, Result<SalesRequestAppItemDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public LinkSalesRequestDraftHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<SalesRequestAppItemDto>> Handle(
        LinkSalesRequestDraftCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return SalesRequestErrors.NotOwner;

        var entity = await _db.SalesRequests
            .FirstOrDefaultAsync(x => x.Id == request.RequestId && !x.IsDeleted, cancellationToken);
        if (entity is null)
            return SalesRequestErrors.NotFound;
        if (!entity.IsOwnedBy(userId.Value))
            return SalesRequestErrors.NotOwner;

        var orderOwned = await _db.Orders.AsNoTracking()
            .AnyAsync(o => o.Id == request.OrderId && o.SellerId == userId, cancellationToken);
        if (!orderOwned)
            return SalesRequestErrors.NotFound;

        // idempotent: نفس الربط مسموح
        if (entity.LinkedOrderId == request.OrderId)
            return ToDto(entity);

        if (entity.LinkedOrderId is int other && other != request.OrderId)
            return SalesRequestErrors.AlreadyLinked;

        var taken = await _db.SalesRequests.AsNoTracking()
            .AnyAsync(x => x.LinkedOrderId == request.OrderId && x.Id != entity.Id, cancellationToken);
        if (taken)
            return SalesRequestErrors.AlreadyLinked;

        var at = _clock.UtcNowOffset;
        var prev = entity.Status;
        try
        {
            entity.LinkDraftOrder(request.OrderId, at);
        }
        catch (InvalidOperationException)
        {
            return SalesRequestErrors.IllegalTransition;
        }

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, SalesRequestTimelineEventType.DraftLinked, userId.Value, at,
            prev, entity.Status, entity.TargetEmployeeId, entity.TargetEmployeeId, null, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(
            _db, entity, SalesRequestChangeType.DraftLinked, at);

        var save = await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct), cancellationToken);
        if (save.IsFailed)
            return save.Error!;

        return ToDto(entity);
    }

    private static SalesRequestAppItemDto ToDto(Domain.Entities.SalesRequestAggregate.SalesRequest x) => new()
    {
        RequestId = x.Id,
        CustomerName = x.CustomerName,
        CustomerPhone = x.CustomerPhone,
        Status = x.Status,
        StatusLabel = SalesRequestStatusLabels.For(x.Status),
        TabKey = SalesRequestTabMap.DisplayTabForStatus(x.Status),
        SourceType = x.SourceType,
        SourceTypeLabel = SalesRequestSourceLabels.For(x.SourceType),
        BranchId = x.BranchId,
        TargetEmployeeId = x.TargetEmployeeId,
        LinkedOrderId = x.LinkedOrderId,
        CreatedAt = x.CreatedAt,
        StatusChangedAtUtc = x.StatusChangedAtUtc,
        ViewedAtUtc = x.ViewedAtUtc,
        LastStatusNote = x.LastStatusNote,
        IsReturned = x.Status == SalesRequestStatus.Returned
    };
}
