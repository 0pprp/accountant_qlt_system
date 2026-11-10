using Application.SalesEmployees.Common;

namespace Application.SalesEmployees.Queries.Admin.List;

public sealed class ListSalesEmployeesQuery : IRequest<Result<ListSalesEmployeesResponse>>
{
    public int? BranchId { get; init; }
    public string? Search { get; init; }
    public string? ShiftState { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Sort { get; init; }
}
