using Application.SalesEmployeeTracking.Common;

namespace Application.SalesEmployeeTracking.Queries.Admin.GetRoute;

public sealed class GetSalesEmployeeRouteQuery : IRequest<Result<SalesEmployeeRouteResponse>>
{
    public int EmployeeId { get; init; }
    public int? BranchId { get; init; }
    public DateOnly? Date { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}
