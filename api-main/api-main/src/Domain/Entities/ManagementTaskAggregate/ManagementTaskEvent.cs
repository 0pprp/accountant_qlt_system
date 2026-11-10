using Domain.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Domain.Entities.ManagementTaskAggregate;

public class ManagementTaskEvent : Entity
{
    private ManagementTaskEvent()
    {
    }

    public ManagementTaskEvent(
        int taskId,
        ManagementTaskEventType eventType,
        DateTimeOffset atUtc,
        int? actorUserId,
        string? actorDisplayName,
        int? targetUserId,
        string? targetDisplayName,
        string? note,
        ManagementTaskStatus? previousStatus,
        ManagementTaskStatus? newStatus)
    {
        TaskId = taskId;
        EventType = eventType;
        AtUtc = atUtc;
        ActorUserId = actorUserId;
        ActorDisplayName = actorDisplayName;
        TargetUserId = targetUserId;
        TargetDisplayName = targetDisplayName;
        Note = note;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }

    public int TaskId { get; private set; }
    public ManagementTaskEventType EventType { get; private set; }
    public DateTimeOffset AtUtc { get; private set; }
    public int? ActorUserId { get; private set; }
    public string? ActorDisplayName { get; private set; }
    public int? TargetUserId { get; private set; }
    public string? TargetDisplayName { get; private set; }
    public string? Note { get; private set; }
    public ManagementTaskStatus? PreviousStatus { get; private set; }
    public ManagementTaskStatus? NewStatus { get; private set; }

    public ManagementTask? Task { get; private set; }
}
