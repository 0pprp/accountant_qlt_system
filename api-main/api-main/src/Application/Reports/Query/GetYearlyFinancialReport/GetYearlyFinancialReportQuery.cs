namespace Application.Reports.Query.GetYearlyFinancialReport;

public record GetYearlyFinancialReportQuery : IRequest<Result<GetYearlyFinancialReportResponse>>
{
    public int? Year { get; set; }
    public required int BranchId { get; set; }
}
