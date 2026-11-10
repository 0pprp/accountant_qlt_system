using Application.Common.Interfaces;
using Domain.Entities.SalesRequestAggregate;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Application.SalesRequests.Common;

public static class SalesRequestTimelineWriter
{
    public static void Append(
        IApplicationDbContext db,
        int salesRequestId,
        SalesRequestTimelineEventType eventType,
        int actorUserId,
        DateTimeOffset atUtc,
        SalesRequestStatus? previousStatus,
        SalesRequestStatus? newStatus,
        int? previousEmployeeId,
        int? newEmployeeId,
        string? reason,
        Guid operationId)
    {
        db.SalesRequestTimelineEntries.Add(new SalesRequestTimelineEntry(
            salesRequestId,
            eventType,
            actorUserId,
            atUtc,
            previousStatus,
            newStatus,
            previousEmployeeId,
            newEmployeeId,
            reason,
            operationId));
    }
}
