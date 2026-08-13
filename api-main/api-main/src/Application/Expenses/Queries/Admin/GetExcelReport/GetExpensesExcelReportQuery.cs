namespace Application.Expenses.Queries.Admin.GetExcelReport;

public record GetExpensesExcelReportQuery : IRequest<Result<GetExpensesExcelReportResponse>>
{
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
