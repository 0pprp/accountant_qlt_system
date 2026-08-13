namespace Application.Orders.Queries.Admin.GetSummary;

public record GetOrderSummaryResponse
{
    public int Id { get; set; }
    public double BuyAmount { get; set; }
    public double SellAmount { get; set; }
    public double PaidAmount { get; set; }
    public double OverdueAmount { get; set; }
    public double DailyInstallmentAmount { get; set; }
    public double UnpaidAmount => SellAmount - PaidAmount;
}