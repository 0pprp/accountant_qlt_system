namespace Application.Products.Commands.Admin.Create;

public record CreateProductCommand : IRequest<Result>
{
    public required string Name { get; set; }
    public int RemainingCount { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int WarehouseId { get; set; }
    public int BranchId { get; set; }
}