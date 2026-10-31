using Application.Common.Interfaces;
using Application.SalesRatings.Common;

namespace Application.SalesRatings.Queries.App.GetByLevel;

public class GetSalesRatingsByLevelHandler : IRequestHandler<GetSalesRatingsByLevelQuery, Result<List<GetSalesRatingsByLevelResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetSalesRatingsByLevelHandler(IApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<List<GetSalesRatingsByLevelResponse>>> Handle(GetSalesRatingsByLevelQuery request, CancellationToken cancellationToken)
    {
        if (CustomerRatingCalculator.IsCardLevel(request.Level) == false)
            return SalesRatingErrors.InvalidLevel;

        var level = CustomerRatingCalculator.NormalizeCardLevel(request.Level);
        var asOf = _dateTimeProvider.Today;
        var customers = await SalesRatingCustomerProjector.GetRatedCustomersAsync(_dbContext, request.UserId, asOf, cancellationToken);

        var filtered = customers
            .Where(x => x.AmountRemaining > 0)
            .Where(x => x.Level == level)
            .Select(x => new GetSalesRatingsByLevelResponse
            {
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                Phone = x.Phone,
                Level = x.Level,
                RatingLabel = x.RatingLabel,
                Score = x.Score,
                AmountRemaining = x.AmountRemaining
            })
            .OrderBy(x => x.CustomerName)
            .ToList();

        return filtered;
    }
}
