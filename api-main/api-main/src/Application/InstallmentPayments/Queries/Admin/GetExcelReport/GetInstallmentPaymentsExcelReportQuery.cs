namespace Application.InstallmentPayments.Queries.Admin.GetExcelReport;

public record GetInstallmentPaymentsExcelReportQuery : IRequest<Result<GetInstallmentPaymentsExcelReportResponse>>
{
    public int BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}