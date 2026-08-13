using Application.Customers.Queries.Admin.GetPaginated;

namespace Application.Customers.Queries.Admin.GetExcelReport;

public record GetCustomersExcelReportQuery(GetPaginatedCustomersFilter Filter) : IRequest<Result<GetCustomersExcelReportResponse>>;