namespace Application.Products.Queries.Admin.GetExcelReport;

public record GetProductsExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetProductExcelReportItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int RemainingCount { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public required string CreatorName { get; set; }
    public required string WarehouseName { get; set; }
    public required string ProductCategoryName { get; set; }
}