namespace Application.Products.Commands.Admin.Update;

public record UpdateProductCommand : IRequest<Result>
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int RemainingCount { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int WarehouseId { get; set; }
}

public record UpdateProductDto
{
    public required string Name { get; set; }
    public int RemainingCount { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int WarehouseId { get; set; }
}