using Application.Common.Models;

namespace Application.ProductCategories.Queries.Admin.GetPaginated;

public record GetPaginatedProductCategoriesResponse
{
    public required PaginatedList<GetPaginatedProductCategoriesItem> Items { get; set; }
    public int TotalProductsCount { get; set; }
}

public record GetPaginatedProductCategoriesItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int ProductsCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}