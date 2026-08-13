namespace Application.Purchases.Queries.Admin.GetExcelReport;

public record GetPurchasesExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetPurchaseExcelReportItem
{
    public int Id { get; set; }
    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public required string SafeName { get; set; }
    public int PurchaseItemsCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}