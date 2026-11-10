using Application.ManagementSummary.Common;

namespace Application.ManagementSummary.Queries.Admin.GetDetails;

public record GetManagementSummaryDetailsQuery : IRequest<Result<ManagementSummaryDetailsResponse>>
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int Day { get; init; }
    public string Metric { get; init; } = "receipts";
    public int BranchId { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
