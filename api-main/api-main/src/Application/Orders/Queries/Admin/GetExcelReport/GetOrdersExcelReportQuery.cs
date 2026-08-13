using Application.Orders.Queries.Admin.GetPaginated;

namespace Application.Orders.Queries.Admin.GetExcelReport;

public record GetOrdersExcelReportQuery : IRequest<Result<GetOrdersExcelReportResponse>>
{
    public required GetOrdersPaginatedFilter Filter { get; set; }
    public required List<string> SelectedColumns { get; set; }
}