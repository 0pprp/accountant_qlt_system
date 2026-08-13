using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.Customers.Queries.Admin.GetPaginated;

public record GetPaginatedCustomersQuery : IRequest<Result<PaginatedList<GetPaginatedCustomersResponse>>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedCustomersFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedCustomersFilter
{
    public int BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}