using Application.SalesEmployees.Common;

namespace Application.SalesEmployees.Queries.Admin.GetRatingSummary;

public sealed class GetSalesEmployeeRatingSummaryQuery : IRequest<Result<SalesEmployeeRatingSummaryDto>>
{
    public int EmployeeId { get; init; }
    public int? BranchId { get; init; }
    public string? Bucket { get; init; }
}
