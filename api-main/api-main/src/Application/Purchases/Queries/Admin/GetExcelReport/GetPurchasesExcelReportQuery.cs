namespace Application.Purchases.Queries.Admin.GetExcelReport;

public record GetPurchasesExcelReportQuery : IRequest<Result<GetPurchasesExcelReportResponse>>
{
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}