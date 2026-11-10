using Application.SalesEmployeeTracking.Common;

namespace Application.SalesEmployeeTracking.Queries.Admin.GetLatest;

public sealed class GetSalesEmployeeLiveLocationsQuery : IRequest<Result<SalesEmployeeLiveLocationsResponse>>
{
    public int? BranchId { get; init; }
    public int? EmployeeId { get; init; }
}
