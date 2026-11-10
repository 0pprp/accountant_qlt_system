using Application.ManagementSummary.Common;

namespace Application.ManagementSummary.Queries.Admin.GetMatrix;

public record GetManagementSummaryMatrixQuery : IRequest<Result<ManagementSummaryMatrixResponse>>
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string Metric { get; init; } = "receipts";
    public int? BranchId { get; init; }
}
