namespace Application.SalesRatings.Queries.App.GetByLevel;

public record GetSalesRatingsByLevelQuery : IRequest<Result<List<GetSalesRatingsByLevelResponse>>>
{
    public int UserId { get; set; }
    public required string Level { get; set; }
}

public record GetSalesRatingsByLevelResponse
{
    public int CustomerId { get; set; }
    public required string CustomerName { get; set; }
    public string? Phone { get; set; }
    public required string Level { get; set; }
    public required string RatingLabel { get; set; }
    public int Score { get; set; }
    public double? AmountRemaining { get; set; }
}
