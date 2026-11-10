namespace Application.SalesRequests.Queries.Admin.AssignableEmployees;

public record GetAssignableEmployeesQuery(int BranchId, string? Search = null)
    : IRequest<Result<List<AssignableEmployeeItem>>>;

public record AssignableEmployeeItem
{
    public int Id { get; init; }
    public required string FullName { get; init; }
    public string? Username { get; init; }
}
