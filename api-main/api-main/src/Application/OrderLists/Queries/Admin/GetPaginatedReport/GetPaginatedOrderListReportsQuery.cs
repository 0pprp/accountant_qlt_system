using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.OrderLists.Queries.Admin.GetPaginatedReport;

public record GetPaginatedOrderListReportsQuery : IRequest<Result<GetPaginatedOrderListReportsResponse>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedOrderListReportsFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedOrderListReportsFilter
{
    public int BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}