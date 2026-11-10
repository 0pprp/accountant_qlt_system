using Domain.Common;
using Domain.Entities.SalesRequestAggregate.Enums;

namespace Domain.Entities.SalesRequestAggregate;

/// <summary>
/// سجل حركة append-only — بلا tokens أو مستندات أو مبالغ.
/// </summary>
public class SalesRequestTimelineEntry : Entity
{
    private SalesRequestTimelineEntry()
    {
    }

    public SalesRequestTimelineEntry(
        int salesRequestId,
        SalesRequestTimelineEventType eventType,
        int actorUserId,
        DateTimeOffset occurredAtUtc,
        SalesRequestStatus? previousStatus,
        SalesRequestStatus? newStatus,
        int? previousEmployeeId,
        int? newEmployeeId,
        string? reason,
        Guid operationId)
    {
        SalesRequestId = salesRequestId;
        EventType = eventType;
        ActorUserId = actorUserId;
        OccurredAtUtc = occurredAtUtc;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        PreviousEmployeeId = previousEmployeeId;
        NewEmployeeId = newEmployeeId;
        Reason = reason;
        OperationId = operationId;
    }

    public int SalesRequestId { get; private set; }
    public SalesRequestTimelineEventType EventType { get; private set; }
    public int ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public SalesRequestStatus? PreviousStatus { get; private set; }
    public SalesRequestStatus? NewStatus { get; private set; }
    public int? PreviousEmployeeId { get; private set; }
    public int? NewEmployeeId { get; private set; }
    public string? Reason { get; private set; }
    public Guid OperationId { get; private set; }

    public SalesRequest? SalesRequest { get; set; }
}
