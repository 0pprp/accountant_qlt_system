namespace Application.Reports.Query.GetYearlyFinancialReport;

public record GetYearlyFinancialReportResponse
{
    public double TotalSellAmount { get; set; }
    public double TotalPurchaseAmount { get; set; }
    public double TotalInstallmentPaymentAmount { get; set; }
    public required List<MonthlyFinancialValue> SellAmounts { get; set; }
    public required List<MonthlyFinancialValue> PurchaseAmounts { get; set; }
    public required List<MonthlyFinancialValue> InstallmentPaymentAmounts { get; set; }
}

public record MonthlyFinancialValue
{
    public int Month { get; set; }
    public double Value { get; set; }
}
