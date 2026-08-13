namespace Application.ProductCategories.Queries.App.GetPaginated;

public record GetPaginatedProductCategoriesResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}