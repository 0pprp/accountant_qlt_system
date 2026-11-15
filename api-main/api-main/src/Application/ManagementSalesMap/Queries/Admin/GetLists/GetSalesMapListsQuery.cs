using Application.ManagementSalesMap.Common;

namespace Application.ManagementSalesMap.Queries.Admin.GetLists;

public record GetSalesMapListsQuery : IRequest<Result<SalesMapListsResponse>>
{
    public int ProvinceId { get; init; }
}
