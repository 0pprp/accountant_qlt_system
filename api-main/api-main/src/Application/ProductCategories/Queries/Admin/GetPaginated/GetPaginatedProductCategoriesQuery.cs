using Application.Common.Models;
using Application.Common.Models.Sorting;

namespace Application.ProductCategories.Queries.Admin.GetPaginated;

public record GetPaginatedProductCategoriesQuery : IRequest<Result<GetPaginatedProductCategoriesResponse>>
{
    public required Pagination Pagination { get; set; }
    public required GetPaginatedProductCategoriesFilter Filter { get; set; }
    public List<SortCriterion>? SortCriteria { get; set; }
}

public record GetPaginatedProductCategoriesFilter
{
    public int BranchId { get; set; }
    public string? SearchTerm { get; set; }
}