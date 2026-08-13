namespace Application.Reports.Query.GetSafeReport;

public record GetSafeReportQuery(int BranchId) : IRequest<Result<GetSafeReportResponse>>;