namespace Application.SalesRatings.Queries.App.GetSummary;

public record GetSalesRatingsSummaryQuery(int UserId) : IRequest<Result<GetSalesRatingsSummaryResponse>>;

public record GetSalesRatingsSummaryResponse
{
    /// <summary>متوسط درجات زبائن البائع الذين عليهم باقي &gt; 0.</summary>
    public double AverageScore { get; set; }
    public string AverageLabel { get; set; } = "ضعيف";
    public int RatedCustomerCount { get; set; }
    public int WeakCount { get; set; }
    public int LaggingCount { get; set; }
    public int BadCount { get; set; }
}
