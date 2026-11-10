using Application.Common.Models;
using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Queries.Admin.List;

public record GetManagementTasksQuery : IRequest<Result<PaginatedList<ManagementTaskListItemDto>>>
{
    public required Pagination Pagination { get; init; }
    public int? BranchId { get; init; }
    public ManagementTaskStatus? Status { get; init; }
    public string? Search { get; init; }
    public bool InboxForMe { get; init; }
}
