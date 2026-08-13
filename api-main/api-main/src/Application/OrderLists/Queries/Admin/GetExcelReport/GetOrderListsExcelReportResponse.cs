namespace Application.OrderLists.Queries.Admin.GetExcelReport;

public record GetOrderListsExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetOrderListsExcelReportItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string MandobName { get; set; }
    public required string MotabaName { get; set; }
    public required string BranchName { get; set; }
    public int CustomersCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}