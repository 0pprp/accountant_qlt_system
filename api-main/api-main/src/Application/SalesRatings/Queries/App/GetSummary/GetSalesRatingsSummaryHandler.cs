using Application.Common.Interfaces;
using Application.SalesRatings.Common;

namespace Application.SalesRatings.Queries.App.GetSummary;

public class GetSalesRatingsSummaryHandler : IRequestHandler<GetSalesRatingsSummaryQuery, Result<GetSalesRatingsSummaryResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetSalesRatingsSummaryHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GetSalesRatingsSummaryResponse>> Handle(GetSalesRatingsSummaryQuery request, CancellationToken cancellationToken)
    {
        var asOf = _dateTimeProvider.Today;
        var customers = await SalesRatingCustomerProjector.GetRatedCustomersAsync(_dbContext, request.UserId, asOf, cancellationToken);
        var openBalance = customers.Where(x => x.AmountRemaining > 0).ToList();
        var average = openBalance.Count == 0
            ? 0
            : openBalance.Average(x => x.Score);

        return new GetSalesRatingsSummaryResponse
        {
            AverageScore = Math.Round(average, 2, MidpointRounding.AwayFromZero),
            AverageLabel = RatingAggregator.ClassifyAverageLabel(average),
            RatedCustomerCount = openBalance.Count,
            WeakCount = openBalance.Count(x => x.Score == 0),
            LaggingCount = openBalance.Count(x => x.Score == -5),
            BadCount = openBalance.Count(x => x.Score <= -10)
        };
    }
}
