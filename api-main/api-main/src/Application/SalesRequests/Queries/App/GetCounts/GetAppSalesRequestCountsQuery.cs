namespace Application.SalesRequests.Queries.App.GetCounts;

public record GetAppSalesRequestCountsQuery
    : IRequest<Result<List<AppSalesRequestBinCount>>>;

public record AppSalesRequestBinCount
{
    public required string Key { get; init; }
    public required string Label { get; init; }
    public int Count { get; init; }
}
