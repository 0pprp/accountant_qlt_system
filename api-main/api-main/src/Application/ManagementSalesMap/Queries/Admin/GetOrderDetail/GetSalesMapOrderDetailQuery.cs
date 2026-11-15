using Application.ManagementSalesMap.Common;

namespace Application.ManagementSalesMap.Queries.Admin.GetOrderDetail;

public record GetSalesMapOrderDetailQuery : IRequest<Result<SalesMapOrderDetailDto>>
{
    public int OrderId { get; init; }
}
