namespace Application.Products.Queries.Admin.GetExcelReport;

public record GetProductsExcelReportQuery(int BranchId) : IRequest<Result<GetProductsExcelReportResponse>>;