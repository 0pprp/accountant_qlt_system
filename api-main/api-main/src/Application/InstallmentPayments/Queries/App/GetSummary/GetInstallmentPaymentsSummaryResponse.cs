namespace Application.InstallmentPayments.Queries.App.GetSummary;

public record GetInstallmentPaymentsSummaryResponse
{
    public int TotalCount { get; set; }
    public double TotalAmount { get; set; }
}