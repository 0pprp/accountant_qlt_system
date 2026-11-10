using Application.ManagementTasks.Realtime;
using Domain.Entities.ManagementTaskAggregate.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs;

public class SignalRManagementTaskRealtimePublisher : IManagementTaskRealtimePublisher
{
    private readonly IHubContext<ManagementTaskHub> _hubContext;

    public SignalRManagementTaskRealtimePublisher(IHubContext<ManagementTaskHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(
        int taskId,
        int revision,
        ManagementTaskChangeType changeType,
        int branchId,
        int holderId,
        int assigneeId,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            taskId,
            revision,
            changeType = (byte)changeType,
            branchId,
            holderId,
            assigneeId
        };

        var groups = new List<string>
        {
            ManagementTaskHub.BranchGroup(branchId),
            ManagementTaskHub.UserGroup(holderId.ToString()),
            ManagementTaskHub.UserGroup(assigneeId.ToString())
        };

        return _hubContext.Clients
            .Groups(groups)
            .SendAsync(ManagementTaskHub.ChangedMethod, payload, cancellationToken);
    }
}
