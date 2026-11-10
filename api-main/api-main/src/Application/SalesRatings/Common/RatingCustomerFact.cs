namespace Application.SalesRatings.Common;

public sealed class RatingCustomerFact
{
    public int CustomerId { get; init; }
    public int? OrderListId { get; init; }
    public int? SellerId { get; init; }
    public double AmountTotalSales { get; init; }
    public double ReceiptsTotal { get; init; }
    public double AmountDaySales { get; init; }
    public DateOnly? SaleDate { get; init; }
    public DateOnly? LastPaymentDate { get; init; }
    public bool IsLegal { get; set; }

    public double AmountRemaining =>
        Math.Round(AmountTotalSales - ReceiptsTotal, MidpointRounding.AwayFromZero);

    public bool HasOpenBalance => AmountRemaining > 0;
}
