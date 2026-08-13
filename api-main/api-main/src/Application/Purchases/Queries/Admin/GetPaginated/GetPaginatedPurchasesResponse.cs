using Application.Products.Common;
using Domain.Entities.SafeAggregate.Enums;

namespace Application.Purchases.Queries.Admin.GetPaginated;

public record GetPaginatedPurchasesResponse
{
    public int Id { get; set; }
    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public SafeType SafeType { get; set; }
    public int PurchaseItemsCount { get; set; }
    public required List<PurchaseItemDto> PurchaseItems { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public record PurchaseItemDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public double Amount { get; set; }
    public ProductDto? Product { get; set; }
    public string? ForeignProductName { get; set; }
}