using Application.ManagementSalesMap.Common;

namespace Application.ManagementSalesMap.Queries.Admin.GetPoints;

public record GetSalesMapPointsQuery : IRequest<Result<SalesMapPointsResponse>>
{
    public int ProvinceId { get; init; }
    public IReadOnlyList<int>? ListIds { get; init; }
    public SalesMapStatusFilter Status { get; init; } = SalesMapStatusFilter.All;
}
