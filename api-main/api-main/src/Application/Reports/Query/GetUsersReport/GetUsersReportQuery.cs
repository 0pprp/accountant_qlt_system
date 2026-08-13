namespace Application.Reports.Query.GetUsersReport;

public record GetUsersReportQuery(int BranchId) : IRequest<Result<GetUsersReportResponse>>;
