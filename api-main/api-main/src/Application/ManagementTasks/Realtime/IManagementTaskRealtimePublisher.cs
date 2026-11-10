using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Realtime;

public interface IManagementTaskRealtimePublisher
{
    Task PublishAsync(
        int taskId,
        int revision,
        ManagementTaskChangeType changeType,
        int branchId,
        int holderId,
        int assigneeId,
        CancellationToken cancellationToken);
}
