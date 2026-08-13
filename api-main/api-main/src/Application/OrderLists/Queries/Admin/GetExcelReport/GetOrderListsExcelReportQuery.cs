using Application.OrderLists.Queries.Admin.GetPaginated;

namespace Application.OrderLists.Queries.Admin.GetExcelReport;

public record GetOrderListsExcelReportQuery(GetPaginatedOrderListsFilter Filter) : IRequest<Result<GetOrderListsExcelReportResponse>>;