namespace Application.Orders.Queries.App.GetOrderInstallmentPaymentsPdfReport;

public record GetOrderInstallmentPaymentsPdfReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetOrderInstallmentPaymentsPdfReportItem
{
    public int? Id { get; set; }
    public required string Date { get; set; }
    public double? Amount { get; set; }
    public required string Status { get; set; }
    public string? Description { get; set; }
}