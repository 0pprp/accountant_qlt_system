using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Commands.Admin.Create;

public record CreateManagementTaskCommand : IRequest<Result<ManagementTaskActionResultDto>>
{
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;
    public ManagementTaskPriority Priority { get; init; } = ManagementTaskPriority.Medium;
    public DateOnly DueDate { get; init; }
    public DateTimeOffset? DueAtUtc { get; init; }
    public int BranchId { get; init; }
    public int AssigneeUserId { get; init; }
    public int? CustomerId { get; init; }
    public int? OrderId { get; init; }
}
