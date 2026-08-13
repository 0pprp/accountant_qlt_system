namespace Application.Orders.Queries.Admin.GetExcelReport;

public record GetOrdersExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetOrdersExcelReportItem
{
    public int Id { get; set; }
    public required string CustomerFullName { get; set; }
    public double SellAmount { get; set; }
    public string? SellerName { get; set; }
    public string? SaleDate { get; set; }
}