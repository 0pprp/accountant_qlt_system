using Application.Common.Models;
using Application.Common.Models.Sorting;
using Domain.Entities.ActivityLogAggregate.Enums;

namespace Application.ActivityLogs.Queries.Admin.GetPaginated;

public record GetPaginatedActivityLogsQuery : IRequest<Result<PaginatedList<GetPaginatedActivityLogsResponse>>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedActivityLogsFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedActivityLogsFilter
{
    public required int BranchId { get; set; }
    public ActivityType? ActivityType { get; set; }
    public TargetEntityType? TargetEntityType { get; set; }
    public int? UserId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
