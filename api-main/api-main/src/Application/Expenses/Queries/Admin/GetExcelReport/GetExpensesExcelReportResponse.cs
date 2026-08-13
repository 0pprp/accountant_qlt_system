namespace Application.Expenses.Queries.Admin.GetExcelReport;

public record GetExpensesExcelReportResponse
{
    public required byte[] Data { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}

public record GetExpenseExcelReportItem
{
    public int Id { get; set; }
    public int FactorNumber { get; set; }
    public double TotalAmount { get; set; }
    public required string SafeName { get; set; }
    public int ExpenseItemsCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
