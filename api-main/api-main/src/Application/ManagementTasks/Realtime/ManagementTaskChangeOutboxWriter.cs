using Application.Common.Interfaces;
using Domain.Entities.ManagementTaskAggregate;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Realtime;

public static class ManagementTaskChangeOutboxWriter
{
    public static ManagementTaskChangeOutbox Append(
        IApplicationDbContext db,
        ManagementTask entity,
        ManagementTaskChangeType changeType,
        DateTimeOffset atUtc)
    {
        var row = new ManagementTaskChangeOutbox(
            Guid.NewGuid(),
            entity.Id,
            entity.ConcurrencyStamp,
            atUtc,
            changeType,
            entity.BranchId,
            entity.CurrentHolderUserId,
            entity.AssigneeUserId);

        db.ManagementTaskChangeOutboxes.Add(row);
        return row;
    }
}
