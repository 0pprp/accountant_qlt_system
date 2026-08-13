using Application.Common.Models;

namespace Application.Products.Queries.Admin.GetPaginated;

public record GetPaginatedProductsResponse
{
    public required PaginatedList<GetPaginatedProductsItem> PaginatedProducts { get; set; }
    public int TotalRemainingCount { get; set; }
    public double TotalBuyAmount { get; set; }
    public double TotalSellAmount { get; set; }
}

public record GetPaginatedProductsItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int RemainingCount { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public required string CategoryName { get; set; }
    public string? WarehouseName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public required string CreatorName { get; set; }
}