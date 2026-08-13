namespace Application.InstallmentPayments.Queries.App.GetDailyPdfReport;

public record GetDailyInstallmentPaymentsPdfReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetDailyInstallmentPaymentsReportItem
{
    public int? Id { get; set; }
    public required string CustomerFullName { get; set; }
    public required string Date { get; set; }
    public double? Amount { get; set; }
    public required string Status { get; set; }
    public string? Description { get; set; }
}