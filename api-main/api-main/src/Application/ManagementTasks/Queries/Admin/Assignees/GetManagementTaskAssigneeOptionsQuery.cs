using Application.ManagementTasks.Common;

namespace Application.ManagementTasks.Queries.Admin.Assignees;

public record GetManagementTaskAssigneeOptionsQuery : IRequest<Result<List<ManagementTaskAssigneeOptionDto>>>
{
    public int? BranchId { get; init; }
    public string? Search { get; init; }
}
