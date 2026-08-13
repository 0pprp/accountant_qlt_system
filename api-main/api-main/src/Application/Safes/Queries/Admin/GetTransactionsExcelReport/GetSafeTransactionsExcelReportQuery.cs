namespace Application.Safes.Queries.Admin.GetTransactionsExcelReport;

public record GetSafeTransactionsExcelReportQuery(int SafeId) : IRequest<Result<GetSafeTransactionsExcelReportResponse>>;