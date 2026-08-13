using Application.Common.Models;
using Application.Common.Models.Sorting;
using Domain.Entities.NotificationAggregate.Enums;

namespace Application.Notifications.Queries.Admin.GetPaginated;

public record GetPaginatedNotificationsQuery : IRequest<Result<PaginatedList<GetPaginatedNotificationsResponse>>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedNotificationsFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedNotificationsFilter
{
    public required int BranchId { get; set; }
    public bool? HasRead { get; set; }
    public NotificationActionType? ActionType { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
