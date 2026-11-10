using Application.SalesSummary.Common;

namespace Application.SalesSummary.Queries.Admin.GetDetails;

public sealed class GetSalesSummaryDetailsQuery : IRequest<Result<SalesSummaryDetailsResponse>>
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int Day { get; init; }
    public int BranchId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string? Metric { get; init; }
}
