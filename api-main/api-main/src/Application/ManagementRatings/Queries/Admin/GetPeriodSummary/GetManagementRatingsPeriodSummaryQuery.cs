using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetPeriodSummary;

public record GetManagementRatingsPeriodSummaryQuery : IRequest<Result<ManagementRatingsPeriodSummaryResponse>>
{
    public int BranchId { get; init; }
    public string Scope { get; init; } = "branch";
    public int ResourceId { get; init; }
}
