using Application.Common.Models.KeysetPagination;

namespace Application.Products.Queries.App.GetPaginated;

public record GetPaginatedProductsQuery : IRequest<Result<List<GetPaginatedProductsResponse>>>
{
    public required KeysetPagination<DateTimeOffset?> Pagination { get; set; }
    public required GetPaginatedProductsFilter Filter { get; set; }
}

public record GetPaginatedProductsFilter
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
}