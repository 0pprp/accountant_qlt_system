using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.App.View;

public class ViewSalesRequestHandler : IRequestHandler<ViewSalesRequestCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public ViewSalesRequestHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(ViewSalesRequestCommand request, CancellationToken cancellationToken)
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

        var at = _clock.UtcNowOffset;
        var prev = entity.Status;
        var statusChanged = entity.MarkViewed(at);
        SalesRequestTimelineWriter.Append(
            _db, entity.Id,
            statusChanged ? SalesRequestTimelineEventType.Viewed : SalesRequestTimelineEventType.ViewSeen,
            userId.Value, at, prev, entity.Status, entity.TargetEmployeeId, entity.TargetEmployeeId, null, Guid.NewGuid());
        if (statusChanged)
        {
            SalesRequestChangeOutboxWriter.Append(
                _db, entity, SalesRequestChangeType.StatusChanged, at);
        }

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct), cancellationToken);
    }
}
