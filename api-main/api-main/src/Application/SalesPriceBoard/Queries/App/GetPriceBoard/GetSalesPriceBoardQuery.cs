namespace Application.SalesPriceBoard.Queries.App.GetPriceBoard;

public record GetSalesPriceBoardQuery : IRequest<Result<List<GetSalesPriceBoardResponse>>>;

public record GetSalesPriceBoardResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double SellAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public int RemainingCount { get; set; }
    public required string CategoryName { get; set; }
}
