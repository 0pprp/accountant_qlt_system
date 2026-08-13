namespace Application.Users.Queries.Admin.GetUserDailyInstallmentCollectionsReport;

public record GetUserDailyInstallmentCollectionsReportQuery : IRequest<Result<GetUserDailyInstallmentCollectionsReportResponse>>
{
    public int UserId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public record GetUserDailyInstallmentCollectionsReportFilter
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}