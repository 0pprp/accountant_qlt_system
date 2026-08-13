using Application.Common.Models.KeysetPagination;
using Application.Common.Models.Sorting;

namespace Application.Customers.Queries.App.GetPaginated;

public record GetPaginatedCustomersQuery : IRequest<Result<List<GetPaginatedCustomersResponse>>>
{
    public required KeysetPagination<DateTimeOffset?> Pagination { get; set; }
    public required GetPaginatedCustomersFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedCustomersFilter
{
    public string? SearchTerm { get; set; }
}