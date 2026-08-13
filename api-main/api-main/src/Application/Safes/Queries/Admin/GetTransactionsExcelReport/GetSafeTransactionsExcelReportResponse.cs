namespace Application.Safes.Queries.Admin.GetTransactionsExcelReport;

public record GetSafeTransactionsExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetSafeTransactionsExcelReportItem
{
    public int Id { get; set; }
    public required string Source { get; set; }
    public required string Destination { get; set; }
    public double Amount { get; set; }
    public required string Type { get; set; }
    public required string Status { get; set; }
    public string? StatusDescription { get; set; }
    public required string Direction { get; set; }
    public required string CreatedAt { get; set; }
}