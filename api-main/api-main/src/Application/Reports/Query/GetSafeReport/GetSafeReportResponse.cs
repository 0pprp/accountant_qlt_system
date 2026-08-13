namespace Application.Reports.Query.GetSafeReport;

public record GetSafeReportResponse
{
    public required SafeReportMetricsDto Today { get; set; }
    public required SafeReportMetricsDto Yesterday { get; set; }
    public required SafeReportMetricsDto LastWeek { get; set; }
    public required SafeReportMetricsDto LastMonth { get; set; }
    public required SafeReportMetricsDto LastYear { get; set; }
}

public record SafeReportMetricsDto
{
    public double TotalWithdrawalAmount { get; set; }
    public double TotalTransferredAmount { get; set; }
    public double TotalEarnedAmount { get; set; }
    public double RemainingCashAmount { get; set; }
}
