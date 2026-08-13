namespace Application.Reports.Query.GetUsersReport;

public record GetUsersReportResponse
{
    public required CountMetric Customers { get; set; }
    public required CountMetric MandobUsers { get; set; }
    public required CountMetric AllUsers { get; set; }
}

public record CountMetric
{
    public int Count { get; set; }
    public double ChangePercent { get; set; }
}
