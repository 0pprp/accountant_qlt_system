using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.Products.Queries.Admin.GetPaginated;

public record GetPaginatedProductsQuery : IRequest<Result<GetPaginatedProductsResponse>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedProductsFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedProductsFilter
{
    public string? SearchTerm { get; set; }
    public int? BranchId { get; set; }
    public int? CategoryId { get; set; }
}