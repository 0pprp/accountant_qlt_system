using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.OrderLists.Queries.Admin.GetPaginated;

public record GetPaginatedOrderListsQuery : IRequest<Result<PaginatedList<GetPaginatedOrderListsResponse>>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedOrderListsFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedOrderListsFilter
{
    public int BranchId { get; set; }
    public string? SearchTerm { get; set; }
}