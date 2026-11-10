using Application.Common.Models;
using Application.ManagementTasks.Common;
using Domain.Entities.ManagementTaskAggregate.Enums;

namespace Application.ManagementTasks.Queries.App.Inbox;

public record GetAppManagementTasksInboxQuery : IRequest<Result<PaginatedList<ManagementTaskListItemDto>>>
{
    public required Pagination Pagination { get; init; }
    public ManagementTaskStatus? Status { get; init; }
}
