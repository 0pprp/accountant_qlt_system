using Application.ManagementRatings.Common;

namespace Application.ManagementRatings.Queries.Admin.GetTrend;

public record GetManagementRatingsTrendQuery : IRequest<Result<ManagementRatingsTrendResponse>>
{
    public int BranchId { get; init; }
    public string Scope { get; init; } = "branch";
    public int ResourceId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
}
