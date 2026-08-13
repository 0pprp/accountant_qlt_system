namespace Application.Safes.Queries.Admin.GetSellersExcelReport;

public record GetSafeSellersExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetSafeSellersExcelReportItem
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public double DeliveredCashAmount { get; set; }
    public double UndeliveredCashAmount { get; set; }
    public string? LastCashDeliveryDate { get; set; }
    public string? LastCashDeliveryDescription { get; set; }
}