using Application.Common.Interfaces;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Common;

public static class ManagementTaskEventWriter
{
    public static void Append(
        IApplicationDbContext db,
        ManagementTask task,
        ManagementTaskEventType eventType,
        DateTimeOffset atUtc,
        int? actorUserId,
        string? actorDisplayName,
        ManagementTaskStatus? previousStatus,
        ManagementTaskStatus? newStatus,
        int? targetUserId = null,
        string? targetDisplayName = null,
        string? note = null)
    {
        db.ManagementTaskEvents.Add(new ManagementTaskEvent(
            task.Id,
            eventType,
            atUtc,
            actorUserId,
            actorDisplayName,
            targetUserId,
            targetDisplayName,
            note,
            previousStatus,
            newStatus));
    }
}
