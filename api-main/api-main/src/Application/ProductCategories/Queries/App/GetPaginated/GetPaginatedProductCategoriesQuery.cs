using Application.Common.Models.KeysetPagination;

namespace Application.ProductCategories.Queries.App.GetPaginated;

public record GetPaginatedProductCategoriesQuery : IRequest<List<GetPaginatedProductCategoriesResponse>>
{
    public required KeysetPagination<DateTimeOffset?> Pagination { get; set; }
    public required GetPaginatedProductCategoriesFilter Filter { get; set; }
}

public record GetPaginatedProductCategoriesFilter
{
    public string? SearchTerm { get; set; }
}