using Application.Common.Interfaces;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Realtime;

public static class SalesRequestChangeOutboxWriter
{
    public static SalesRequestChangeOutbox Append(
        IApplicationDbContext db,
        SalesRequest entity,
        SalesRequestChangeType changeType,
        DateTimeOffset atUtc,
        int? previousTargetEmployeeId = null)
    {
        return Append(db, new SalesRequestChangeMessage(
            entity.Id,
            entity.ConcurrencyStamp,
            changeType,
            entity.BranchId,
            entity.TargetEmployeeId,
            previousTargetEmployeeId,
            atUtc));
    }

    public static SalesRequestChangeOutbox Append(IApplicationDbContext db, SalesRequestChangeMessage message)
    {
        var row = new SalesRequestChangeOutbox(
            Guid.NewGuid(),
            message.RequestId,
            message.Revision,
            message.OccurredAtUtc,
            message.ChangeType,
            message.BranchId,
            message.TargetEmployeeId,
            message.PreviousTargetEmployeeId);

        db.SalesRequestChangeOutboxes.Add(row);
        return row;
    }
}
