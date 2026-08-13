namespace Application.InstallmentPayments.Queries.Admin.GetExcelReport;

public record GetInstallmentPaymentsExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetInstallmentPaymentsExcelReportItem
{
    public int Id { get; set; }
    public required string CustomerFullName { get; set; }
    public required string Date { get; set; }
    public required string SellerFullName { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }
}