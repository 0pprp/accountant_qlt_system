namespace Application.Products.Queries.App.GetPaginated;

public record GetPaginatedProductsResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public required ProductCategoryDto Category { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public record ProductCategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}