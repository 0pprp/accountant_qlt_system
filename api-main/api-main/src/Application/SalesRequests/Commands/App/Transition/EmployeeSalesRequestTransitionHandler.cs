using Application.Common.Interfaces;
using Application.SalesRequests.Common;
using Application.SalesRequests.Realtime;
using Domain.Entities.SalesRequestAggregate.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.SalesRequests.Commands.App.Transition;

public class EmployeeSalesRequestTransitionHandler
    : IRequestHandler<EmployeeSalesRequestTransitionCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public EmployeeSalesRequestTransitionHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(
        EmployeeSalesRequestTransitionCommand request,
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
        if (!entity.CanEmployeeTransition())
            return SalesRequestErrors.IllegalTransition;

        var at = _clock.UtcNowOffset;
        var prev = entity.Status;
        SalesRequestTimelineEventType evt;

        try
        {
            switch (request.Action)
            {
                case EmployeeSalesRequestAction.Prepare:
                    if (string.IsNullOrWhiteSpace(request.Note))
                        return SalesRequestErrors.NoteRequired;
                    entity.Prepare(request.Note, at);
                    evt = SalesRequestTimelineEventType.Prepared;
                    break;
                case EmployeeSalesRequestAction.Pend:
                    if (string.IsNullOrWhiteSpace(request.Note))
                        return SalesRequestErrors.NoteRequired;
                    entity.Pend(request.Note!, at);
                    evt = SalesRequestTimelineEventType.Pend;
                    break;
                case EmployeeSalesRequestAction.Reject:
                    if (string.IsNullOrWhiteSpace(request.Note))
                        return SalesRequestErrors.NoteRequired;
                    entity.Reject(request.Note, at);
                    evt = SalesRequestTimelineEventType.Rejected;
                    break;
                case EmployeeSalesRequestAction.Inspect:
                    entity.Inspect(at);
                    evt = SalesRequestTimelineEventType.Inspected;
                    break;
                default:
                    return SalesRequestErrors.IllegalTransition;
            }
        }
        catch (InvalidOperationException)
        {
            return SalesRequestErrors.IllegalTransition;
        }

        SalesRequestTimelineWriter.Append(
            _db, entity.Id, evt, userId.Value, at,
            prev, entity.Status, entity.TargetEmployeeId, entity.TargetEmployeeId, request.Note, Guid.NewGuid());
        SalesRequestChangeOutboxWriter.Append(
            _db, entity, SalesRequestChangeType.StatusChanged, at);

        return await SalesRequestConcurrency.SaveWithConflictGuardAsync(
            ct => _db.SaveChangesAsync(ct), cancellationToken);
    }
}
