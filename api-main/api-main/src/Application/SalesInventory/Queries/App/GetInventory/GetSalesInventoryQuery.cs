namespace Application.SalesInventory.Queries.App.GetInventory;

public record GetSalesInventoryQuery : IRequest<Result<List<GetSalesInventoryResponse>>>;

public record GetSalesInventoryResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int RemainingCount { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public string? Description { get; set; }
    public required SalesInventoryCategoryDto Category { get; set; }
}

public record SalesInventoryCategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
