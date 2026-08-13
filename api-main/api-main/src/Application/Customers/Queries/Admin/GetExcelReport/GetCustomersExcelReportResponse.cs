namespace Application.Customers.Queries.Admin.GetExcelReport;

public record GetCustomersExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetCustomersExcelReportItem
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string BusinessName { get; set; }
    public required string BusinessAddress { get; set; }
    public string? OrderListName { get; set; }
    public int OrdersCount { get; set; }
    public string? LastInstallmentPaymentDate { get; set; }
}