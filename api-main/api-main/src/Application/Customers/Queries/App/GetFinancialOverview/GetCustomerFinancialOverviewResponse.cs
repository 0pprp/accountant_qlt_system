namespace Application.Customers.Queries.App.GetFinancialOverview;

public record GetCustomerFinancialOverviewResponse
{
    public int CustomerId { get; set; }
    public double TotalPaidAmount { get; set; }
    public double TotalOverdueAmount { get; set; }
    public double TotalSellAmount { get; set; }
    public double TotalRemainingAmount => TotalSellAmount - TotalPaidAmount;
    public double TotalDailyInstallmentAmount { get; set; }
    public int ActiveOrdersCount { get; set; }
    public int CompletedOrdersCount { get; set; }
}
