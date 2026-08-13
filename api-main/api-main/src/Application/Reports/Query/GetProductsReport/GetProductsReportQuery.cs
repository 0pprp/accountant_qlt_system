namespace Application.Reports.Query.GetProductsReport;

public record GetProductsReportQuery(int BranchId) : IRequest<Result<GetProductsReportResponse>>;