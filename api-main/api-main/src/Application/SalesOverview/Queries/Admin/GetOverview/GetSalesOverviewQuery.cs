using Application.SalesOverview.Common;

namespace Application.SalesOverview.Queries.Admin.GetOverview;

public sealed class GetSalesOverviewQuery : IRequest<Result<SalesOverviewResponse>>
{
    public int? BranchId { get; init; }
}
