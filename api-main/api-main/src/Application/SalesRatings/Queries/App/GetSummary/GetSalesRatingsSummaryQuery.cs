namespace Application.SalesRatings.Queries.App.GetSummary;

public record GetSalesRatingsSummaryQuery(int UserId) : IRequest<Result<GetSalesRatingsSummaryResponse>>;

public record GetSalesRatingsSummaryResponse
{
    public int WeakCount { get; set; }
    public int LaggingCount { get; set; }
    public int BadCount { get; set; }
}
