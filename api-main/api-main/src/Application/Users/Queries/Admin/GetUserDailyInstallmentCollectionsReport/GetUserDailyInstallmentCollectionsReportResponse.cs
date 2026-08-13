namespace Application.Users.Queries.Admin.GetUserDailyInstallmentCollectionsReport;

public record GetUserDailyInstallmentCollectionsReportResponse
{
    public required List<GetUserDailyInstallmentCollectionsReportItem> Items { get; set; }
    public double UndeliveredCashAmount { get; set; }
}

public record GetUserDailyInstallmentCollectionsReportItem
{
    public DateOnly Date { get; set; }
    public double TotalInstallmentAmount { get; set; }
    public double TotalDeliveredCashAmount { get; set; }
    public double CumulativeUndeliveredCashAmount { get; set; }
}